using Microsoft.EntityFrameworkCore;

using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.DTOs.Requests;
using backend.DTOs.Responses;

namespace backend.Services
{
    public class ShrineService : IShrineService
    {
        private readonly AppDbContext _context;

        public ShrineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShrineSuggestionDto>> GetSuggestionsByKeywordAsync(
            string keyword, string locale = "en")
        {
            return await _context.ShrineTranslations
                .Where(t => t.Locale == locale && t.Name.Contains(keyword))
                .OrderBy(t => t.Name.StartsWith(keyword) ? 0 : 1)
                .Take(6)
                .Select(t => new ShrineSuggestionDto { Name = t.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<ShrineDto>> GetFeaturedAsync(string locale = "en")
        {
            return await _context.Shrines
                .OrderBy(s => Guid.NewGuid())
                .Take(3)
                .Select(s => new ShrineDto
                {
                    Id = s.Id,
                    Name = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? string.Empty,
                    Prefecture = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.Prefecture)
                        .FirstOrDefault() ?? string.Empty,
                    City = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.City)
                        .FirstOrDefault() ?? string.Empty,
                    Region = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.Region)
                        .FirstOrDefault() ?? string.Empty,
                })
                .ToListAsync();
        }

        public async Task<PagedResult<ShrineDto>> GetShrinesAsync(ShrineSearchRequest request)
        {
            string locale = request.Locale;
            int pageSize = Math.Clamp(request.PageSize, 1, 50);
            int currentPage = Math.Max(1, request.Page);
            int offset = (currentPage - 1) * pageSize;

            var baseQuery = _context.Shrines.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Shrine))
            {
                var matchingIds = _context.ShrineTranslations
                    .Where(t => t.Locale == locale && t.Name.Contains(request.Shrine))
                    .Select(t => t.ShrineId);

                baseQuery = baseQuery.Where(s => matchingIds.Contains(s.Id));
            }

            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                double lat = request.Latitude.Value;
                double lon = request.Longitude.Value;

                var candidates = await FetchCandidatesWithDynamicRadiusAsync(baseQuery, lat, lon);

                var sorted = candidates
                    .OrderBy(c => HaversineDistanceKm(lat, lon, c.Lat, c.Lon))
                    .ToList();

                var pageItems = sorted.Skip(offset).Take(pageSize).ToList();

                var ids = pageItems.Select(c => c.Id).ToList();
                var shrines = await _context.Shrines
                    .Where(s => ids.Contains(s.Id))
                    .Select(s => new ShrineDto
                    {
                        Id = s.Id,
                        Name = s.Translations
                            .Where(t => t.Locale == locale)
                            .Select(t => t.Name)
                            .FirstOrDefault() ?? string.Empty,
                        Prefecture = s.Translations
                            .Where(t => t.Locale == locale)
                            .Select(t => t.Prefecture)
                            .FirstOrDefault() ?? string.Empty,
                        City = s.Translations
                            .Where(t => t.Locale == locale)
                            .Select(t => t.City)
                            .FirstOrDefault() ?? string.Empty,
                        Region = s.Translations
                            .Where(t => t.Locale == locale)
                            .Select(t => t.Region)
                            .FirstOrDefault() ?? string.Empty,
                    })
                    .Take(6)
                    .ToListAsync();

                var ordered = ids
                    .Select(id => shrines.First(s => s.Id == id))
                    .ToList();

                return new PagedResult<ShrineDto>
                {
                    Items = ordered,
                    TotalCount = sorted.Count,
                    CurrentPage = currentPage,
                    PageSize = pageSize,
                };
            }

            var total = await baseQuery.CountAsync();
            var items = await baseQuery
                .OrderBy(s => s.CreatedAt)
                .Skip(offset)
                .Take(pageSize)
                .Select(s => new ShrineDto
                {
                    Id = s.Id,
                    Name = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? string.Empty,
                    Prefecture = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.Prefecture)
                        .FirstOrDefault() ?? string.Empty,
                    City = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.City)
                        .FirstOrDefault() ?? string.Empty,
                    Region = s.Translations
                        .Where(t => t.Locale == locale)
                        .Select(t => t.Region)
                        .FirstOrDefault() ?? string.Empty,
                })
                .ToListAsync();

            return new PagedResult<ShrineDto>
            {
                Items = items,
                TotalCount = total,
                CurrentPage = currentPage,
                PageSize = pageSize,
            };
        }

        private async Task<List<(Guid Id, double Lat, double Lon)>> FetchCandidatesWithDynamicRadiusAsync(
            IQueryable<Shrine> baseQuery, double lat, double lon)
        {
            double[] radii = [50.0, 100.0, 200.0];
            const int minResults = 5;

            foreach (var radiusKm in radii)
            {
                double latDelta = radiusKm / 111.0;
                double lonDelta = radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180.0));

                var candidates = await baseQuery
                    .Where(s =>
                        s.Latitude.HasValue && s.Longitude.HasValue &&
                        s.Latitude >= lat - latDelta && s.Latitude <= lat + latDelta &&
                        s.Longitude >= lon - lonDelta && s.Longitude <= lon + lonDelta)
                    .Select(s => new { s.Id, Lat = s.Latitude!.Value, Lon = s.Longitude!.Value })
                    .ToListAsync();

                if (candidates.Count >= minResults || radiusKm == radii[^1])
                    return candidates.Select(c => (c.Id, c.Lat, c.Lon)).ToList();
            }

            return [];
        }

        private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
