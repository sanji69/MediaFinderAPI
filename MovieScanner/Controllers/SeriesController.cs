using MediaFinder.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeriesController : ControllerBase
    {
        private readonly TmdbService _tmdbService;

        public SeriesController(TmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingSeries([FromQuery] string? language = null)
        {
            var series = await _tmdbService.GetTrendingSeriesAsync(language);
            return Ok(series);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSeriesById(int id, [FromQuery] string? language = null, [FromQuery] string? countryCode = null)
        {
            var series = await _tmdbService.GetSerieAsync(id, language, countryCode);
            return Ok(series);
        }
    }
}
