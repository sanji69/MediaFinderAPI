using MediaFinder.DTOs;
using System.Text.Json;

namespace MediaFinder.Interface
{
    public interface ITmdbService
    {
        Task<List<TmdbTrendingMovieDto>> GetTrendingMoviesAsync(string? language = null);
        Task<List<TmdbTrendingSeriesDto>> GetTrendingSeriesAsync(string? language = null);
        Task<MovieDetailDto> GetMovieAsync(int id, string? language = null, string? countryCode = null);
        Task<SeriesDetailDto> GetSerieAsync(int id, string? language = null, string? countryCode = null);
    }
}
