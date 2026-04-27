using Microsoft.EntityFrameworkCore;

using backend.Data;
using backend.Models;
using backend.Exceptions;
using backend.DTOs;
using backend.DTOs.Requests;
using backend.DTOs.Responses;

using System.Text.Json;

namespace backend.Services
{
    public class ShrineService : IShrineService
    {
        private readonly AppDbContext _context;

        public ShrineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShrineSuggestionDto>> GetSuggestionsByKeywordAsync(string keyword)
        {
            return await _context.Shrines
                .Where(s =>
                    s.Name.Contains(keyword))
                .OrderBy(s => s.Name.StartsWith(keyword) ? 0 : 1)
                .Take(6)
                .Select(s => new ShrineSuggestionDto
                {
                    Name = s.Name,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ShrineDto>> GetFeaturedAsync()
        {
            return await _context.Shrines
                .OrderBy(s => Guid.NewGuid())
                .Take(3)
                .Select(s => new ShrineDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl ?? string.Empty,
                    Prefecture = s.Prefecture ?? string.Empty,
                    City = s.City ?? string.Empty,
                    Region = s.Region ?? string.Empty,
                }).ToListAsync();
        }

        public async Task<IEnumerable<ShrineDto>> GetShrinesAsync(ShrineSearchRequest request)
        {
            var query = _context.Shrines.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Shrine))
                query = query.Where(s =>
                    s.Name.Contains(request.Shrine));

            if (!string.IsNullOrWhiteSpace(request.Location))
                query = query.Where(s =>
                    (s.Prefecture != null && s.Prefecture.Contains(request.Location)) ||
                    (s.Address != null && s.Address.Contains(request.Location)));

            return await query
                .Take(12)
                .Select(s => new ShrineDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ImageUrl = s.ImageUrl ?? string.Empty,
                    Prefecture = s.Prefecture ?? string.Empty,
                    City = s.City ?? string.Empty,
                    Region = s.Region ?? string.Empty,
                })
                .ToListAsync();
        }
    }
}