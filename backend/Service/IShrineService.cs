using backend.DTOs;
using backend.DTOs.Requests;
using backend.DTOs.Responses;

namespace backend.Services
{
    public interface IShrineService
    {
        Task<IEnumerable<ShrineSuggestionDto>> GetSuggestionsByKeywordAsync(string keyword);
        Task<IEnumerable<ShrineDto>> GetFeaturedAsync();
        Task<PagedResult<ShrineDto>> GetShrinesAsync(ShrineSearchRequest request);

    }
}