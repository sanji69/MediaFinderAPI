using System.Net.Http.Headers;
using System.Text.Json;
using MediaFinder.DTOs;
using MediaFinder.Options;
using Microsoft.Extensions.Options;

namespace MediaFinder.Services
{
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly TmdbOptions _options;

        public TmdbService(HttpClient httpClient, IOptions<TmdbOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.BearerToken);
        }

        public async Task<List<TmdbTrendingMovieDto>> GetTrendingMoviesAsync(string language = "fr-FR")
        {
            var response = await _httpClient.GetAsync($"trending/movie/week?language={language}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var results = document.RootElement.GetProperty("results");

            return results.EnumerateArray()
                .Select(movie => new TmdbTrendingMovieDto
                {
                    Id = movie.GetProperty("id").GetInt32(),
                    Title = movie.GetProperty("title").GetString() ?? string.Empty,
                    OriginalTitle = movie.GetProperty("original_title").GetString() ?? string.Empty,
                    Overview = movie.GetProperty("overview").GetString() ?? string.Empty,
                    PosterPath = movie.TryGetProperty("poster_path", out var posterPath) && posterPath.ValueKind != JsonValueKind.Null
                        ? posterPath.GetString()
                        : null,
                    BackdropPath = movie.TryGetProperty("backdrop_path", out var backdropPath) && backdropPath.ValueKind != JsonValueKind.Null
                        ? backdropPath.GetString()
                        : null,
                    ReleaseDate = movie.TryGetProperty("release_date", out var releaseDate) && releaseDate.ValueKind != JsonValueKind.Null
                        ? releaseDate.GetString()
                        : null,
                    VoteAverage = movie.GetProperty("vote_average").GetDouble(),
                    Popularity = movie.GetProperty("popularity").GetDouble()
                })
                .ToList();
        }
    }
}
