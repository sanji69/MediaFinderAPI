using MediaFinder.Interface;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ITmdbService _tmdbService;

        public MoviesController(ITmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingMovies([FromQuery] string? language = null)
        {
            var movies = await _tmdbService.GetTrendingMoviesAsync(language);
            return Ok(movies);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMovieDetails(int id, [FromQuery] string? language = null, [FromQuery] string? countryCode = null)
        {
            var movie = await _tmdbService.GetMovieAsync(id, language, countryCode);
            return Ok(movie);
        }
    }
}
