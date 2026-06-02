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
        public async Task<IActionResult> GetTrendingSeries([FromQuery] string language = "fr-FR")
        {
            var series = await _tmdbService.GetTrendingSeriesAsync(language);
            return Ok(series);
        }
    }
}
