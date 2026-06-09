using MediaFinder.DTOs;
using MediaFinder.Interface;
using MediaFinder.Services.Tmdb;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("multi")]
        public async Task<ActionResult<List<SearchResultDto>>> SearchMulti( [FromQuery] string query, [FromQuery] string? language = null)
        {
            var result = await _searchService.SearchMultiAsync( query, language);

            return Ok(result);
        }

        [HttpGet("person/{personId:int}")]
        public async Task<ActionResult<List<SearchResultDto>>> SearchByPerson( int personId, [FromQuery] string role, [FromQuery] string? language = null)
        {
            var result = await _searchService.SearchByPersonAsync(
                personId,
                role,
                language);

            return Ok(result);
        }

        [HttpGet("genre/{genreId:int}")]
        public async Task<ActionResult<List<SearchResultDto>>> SearchByGenre(int genreId, [FromQuery] string sourceMediaType, [FromQuery] string? language = null)
        {
            var result = await _searchService.SearchByGenreAsync(genreId, sourceMediaType, language);

            return Ok(result);
        }
    }
}
