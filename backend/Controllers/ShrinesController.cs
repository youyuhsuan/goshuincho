using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using backend.DTOs.Requests;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShrinesController : ControllerBase
    {
        private readonly IShrineService _shrineService;
        private readonly ILogger<ShrinesController> _logger;

        public ShrinesController(
            IShrineService shrineService,
            ILogger<ShrinesController> logger)
        {
            _shrineService = shrineService;
            _logger = logger;
        }

        /// GET: api/shrines/suggestions?keyword=
        /// <summary>
        /// Get shrine name suggestions for autocomplete
        /// </summary>
        /// <response code="200">Returns shrine suggestions</response>
        /// <response code="400">Keyword is required</response>
        [ProducesResponseType(typeof(IEnumerable<ShrineSuggestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("suggestions")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ShrineSuggestionDto>>> GetSuggestions(
            [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("keyword is required");

            var suggestions = await _shrineService.GetSuggestionsByKeywordAsync(keyword);
            return Ok(suggestions);
        }

        /// GET: api/shrines/featured
        /// <summary>
        /// Get featured today shrines for homepage
        /// </summary>
        /// <response code="200">Returns featured shrines</response>
        [ProducesResponseType(typeof(IEnumerable<ShrineDto>), StatusCodes.Status200OK)]
        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ShrineDto>>> GetFeaturedShrines()
        {
            var shrines = await _shrineService.GetFeaturedAsync();
            return Ok(shrines);
        }


        /// GET: api/shrines?shrine=&location=
        /// <summary>
        /// Search shrines by keyword and location
        /// </summary>
        /// <response code="200">Returns search results</response>
        [ProducesResponseType(typeof(IEnumerable<ShrineDto>), StatusCodes.Status200OK)]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ShrineDto>>> GetShrines(
            [FromQuery] ShrineSearchRequest request)
        {
            var shrines = await _shrineService.GetShrinesAsync(request);
            return Ok(shrines);
        }
    }
}