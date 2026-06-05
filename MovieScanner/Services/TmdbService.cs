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

        public async Task<List<TmdbTrendingSeriesDto>> GetTrendingSeriesAsync(string language = "fr-FR")
        {
            var response = await _httpClient.GetAsync($"trending/tv/week?language={language}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var results = document.RootElement.GetProperty("results");

            return results.EnumerateArray().Select(series => new TmdbTrendingSeriesDto
            {
                Id = series.GetProperty("id").GetInt32(),
                Name = series.GetProperty("name").GetString() ?? string.Empty,
                OriginalName = series.GetProperty("original_name").GetString() ?? string.Empty,
                Overview = series.GetProperty("overview").GetString() ?? string.Empty,
                PosterPath = series.TryGetProperty("poster_path", out var poster) ? poster.GetString() : null,
                BackdropPath = series.TryGetProperty("backdrop_path", out var backdrop) ? backdrop.GetString() : null,
                FirstAirDate = series.TryGetProperty("first_air_date", out var firstAirDate) ? firstAirDate.GetString() : null,
                VoteAverage = series.TryGetProperty("vote_average", out var vote) ? vote.GetDouble() : 0,
                Popularity = series.TryGetProperty("popularity", out var popularity) ? popularity.GetDouble() : 0
            }).ToList();
        }

        public async Task<MovieDetailDto> GetMovieAsync(int movieId, string language = "fr-FR")
        {
            var response = await _httpClient.GetAsync($"movie/{movieId}?language={language}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var movie = document.RootElement;
            var genres = movie.GetProperty("genres").EnumerateArray().Select(genre => genre.GetProperty("name").GetString() ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();

            return new MovieDetailDto
            {
                Id = movie.GetProperty("id").GetInt32(),
                Title = movie.GetProperty("title").GetString() ?? string.Empty,
                OriginalTitle = movie.GetProperty("original_title").GetString() ?? string.Empty,
                Overview = movie.GetProperty("overview").GetString() ?? string.Empty,
                PosterPath = movie.TryGetProperty("poster_path", out var poster) ? poster.GetString() : null,
                BackdropPath = movie.TryGetProperty("backdrop_path", out var backdrop) ? backdrop.GetString() : null,
                ReleaseDate = movie.TryGetProperty("release_date", out var releaseDate) ? releaseDate.GetString() : null,
                VoteAverage = movie.TryGetProperty("vote_average", out var vote) ? vote.GetDouble() : 0,
                Runtime = movie.TryGetProperty("runtime", out var runtime) ? runtime.GetInt32() : 0,
                Genres = genres
            };
        }

        public async Task<SeriesDetailDto> GetSerieAsync(int serieId, string language = "fr-FR")
        {
            var response = await _httpClient.GetAsync($"tv/{serieId}?language={language}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var series = document.RootElement;
            var genres = series.GetProperty("genres").EnumerateArray().Select(genre => genre.GetProperty("name").GetString() ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();

            return new SeriesDetailDto
            {
                Id = series.GetProperty("id").GetInt32(),

                Name = series.GetProperty("name").GetString() ?? string.Empty,

                OriginalName =
                    series.GetProperty("original_name").GetString() ?? string.Empty,

                Overview =
                    series.GetProperty("overview").GetString() ?? string.Empty,

                PosterPath =
                    series.TryGetProperty("poster_path", out var poster)
                        ? poster.GetString()
                        : null,

                BackdropPath =
                    series.TryGetProperty("backdrop_path", out var backdrop)
                        ? backdrop.GetString()
                        : null,

                FirstAirDate =
                    series.TryGetProperty("first_air_date", out var firstAirDate)
                        ? firstAirDate.GetString()
                        : null,

                VoteAverage =
                    series.TryGetProperty("vote_average", out var vote)
                        ? vote.GetDouble()
                        : 0,

                NumberOfSeasons =
                    series.TryGetProperty("number_of_seasons", out var seasons)
                        ? seasons.GetInt32()
                        : 0,

                NumberOfEpisodes =
                    series.TryGetProperty("number_of_episodes", out var episodes)
                        ? episodes.GetInt32()
                        : 0,

                Genres = genres
            };
        }
    }
}
