using MediaFinder.DTOs;
using MediaFinder.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MediaFinder.Services
{
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly TmdbOptions _options; 
        private readonly LocalizationOptions _localizationOptions;

        public TmdbService(HttpClient httpClient, IOptions<TmdbOptions> options, IOptions<LocalizationOptions> localizationOptions)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.BearerToken);
            _localizationOptions = localizationOptions.Value;
        }

        private string ResolveLanguage(string? language)
        {
            return string.IsNullOrWhiteSpace(language)
                ? _localizationOptions.Language
                : language;
        }

        private string ResolveCountryCode(string? countryCode)
        {
            return string.IsNullOrWhiteSpace(countryCode)
                ? _localizationOptions.CountryCode
                : countryCode;
        }

        private static List<WatchProviderDto> ExtractWatchProviders(JsonElement media, string countryCode)
        {
            var providers = new List<WatchProviderDto>();

            if (!media.TryGetProperty("watch/providers", out var watchProviders) ||
                !watchProviders.TryGetProperty("results", out var results) ||
                !results.TryGetProperty(countryCode, out var countryProviders))
            {
                return providers;
            }

            AddProvidersByType(countryProviders, "flatrate", providers);
            AddProvidersByType(countryProviders, "rent", providers);
            AddProvidersByType(countryProviders, "buy", providers);

            return providers;
        }

        private static void AddProvidersByType(
            JsonElement countryProviders,
            string type,
            List<WatchProviderDto> providers)
        {
            if (!countryProviders.TryGetProperty(type, out var providerList))
            {
                return;
            }

            foreach (var provider in providerList.EnumerateArray())
            {
                var providerId = provider.GetProperty("provider_id").GetInt32();

                if (providers.Any(p => p.Id == providerId && p.Type == type))
                {
                    continue;
                }

                providers.Add(new WatchProviderDto
                {
                    Id = providerId,
                    Name = provider.GetProperty("provider_name").GetString() ?? string.Empty,
                    LogoPath = provider.TryGetProperty("logo_path", out var logo)
                        ? logo.GetString()
                        : null,
                    Type = type
                });
            }
        }

        public async Task<List<TmdbTrendingMovieDto>> GetTrendingMoviesAsync(string? language = null)
        {
            var resolvedLanguage = ResolveLanguage(language);

            var response = await _httpClient.GetAsync($"trending/movie/week?language={resolvedLanguage}");
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

        public async Task<List<TmdbTrendingSeriesDto>> GetTrendingSeriesAsync(string? language = null)
        {
            var resolvedLanguage = ResolveLanguage(language);
            var response = await _httpClient.GetAsync($"trending/tv/week?language={resolvedLanguage}");
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

        public async Task<MovieDetailDto> GetMovieAsync(int movieId, string? language = null, string? countryCode = null)
        {
            var resolvedLanguage = ResolveLanguage(language);
            var resolvedCountryCode = ResolveCountryCode(countryCode);

            var response = await _httpClient.GetAsync($"movie/{movieId}?language={resolvedLanguage}&append_to_response=credits,watch/providers");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var movie = document.RootElement;
            var genres = movie.GetProperty("genres")
                .EnumerateArray()
                .Select(genre => new GenreDto
                {
                    Id = genre.GetProperty("id").GetInt32(),
                    Name = genre.GetProperty("name").GetString() ?? string.Empty
                })
                .Where(genre => !string.IsNullOrWhiteSpace(genre.Name))
                .ToList();

            var directors = movie.GetProperty("credits")
                .GetProperty("crew")
                .EnumerateArray()
                .Where(member =>
                    member.TryGetProperty("job", out var job) &&
                    job.GetString() == "Director")
                .Select(member => new PersonDto
                {
                    Id = member.GetProperty("id").GetInt32(),
                    Name = member.GetProperty("name").GetString() ?? string.Empty
                })
                .Where(person => !string.IsNullOrWhiteSpace(person.Name))
                .GroupBy(person => person.Id)
                .Select(group => group.First())
                .ToList();

            var cast = movie.GetProperty("credits")
                .GetProperty("cast")
                .EnumerateArray()
                .Take(10)
                .Select(member => new CastMemberDto
                {
                    Id = member.GetProperty("id").GetInt32(),
                    Name = member.GetProperty("name").GetString() ?? string.Empty,
                    Character = member.TryGetProperty("character", out var character)
                        ? character.GetString() ?? string.Empty
                        : string.Empty,
                    ProfilePath = member.TryGetProperty("profile_path", out var profile)
                        ? profile.GetString()
                        : null
                })
                .ToList();

            var watchProviders = ExtractWatchProviders(movie, resolvedCountryCode);

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
                Genres = genres,
                Directors = directors,
                Cast = cast,
                WatchProviders = watchProviders
            };
        }

        public async Task<SeriesDetailDto> GetSerieAsync(int serieId, string? language = null, string? countryCode = null)
        {
            var resolvedLanguage = ResolveLanguage(language);
            var resolvedCountryCode = ResolveCountryCode(countryCode);
            var response = await _httpClient.GetAsync($"tv/{serieId}?language={resolvedLanguage}&append_to_response=credits,watch/providers");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var series = document.RootElement;
            var genres = series.GetProperty("genres")
                .EnumerateArray()
                .Select(genre => new GenreDto
                {
                    Id = genre.GetProperty("id").GetInt32(),
                    Name = genre.GetProperty("name").GetString() ?? string.Empty
                })
                .Where(genre => !string.IsNullOrWhiteSpace(genre.Name))
                .ToList();

            var creators = series.GetProperty("created_by")
                .EnumerateArray()
                .Select(creator => new PersonDto
                {
                    Id = creator.GetProperty("id").GetInt32(),
                    Name = creator.GetProperty("name").GetString() ?? string.Empty
                })
                .Where(person => !string.IsNullOrWhiteSpace(person.Name))
                .GroupBy(person => person.Id)
                .Select(group => group.First())
                .ToList();

            var cast = series.GetProperty("credits")
                .GetProperty("cast")
                .EnumerateArray()
                .Take(10)
                .Select(member => new CastMemberDto
                {
                    Id = member.GetProperty("id").GetInt32(),
                    Name = member.GetProperty("name").GetString() ?? string.Empty,
                    Character = member.TryGetProperty("character", out var character)
                        ? character.GetString() ?? string.Empty
                        : string.Empty,
                    ProfilePath = member.TryGetProperty("profile_path", out var profile)
                        ? profile.GetString()
                        : null
                })
                .ToList();

            var watchProviders = ExtractWatchProviders(series, resolvedCountryCode);

            return new SeriesDetailDto
            {
                Id = series.GetProperty("id").GetInt32(),

                Name = series.GetProperty("name").GetString() ?? string.Empty,
                OriginalName = series.GetProperty("original_name").GetString() ?? string.Empty,
                Overview = series.GetProperty("overview").GetString() ?? string.Empty,
                PosterPath = series.TryGetProperty("poster_path", out var poster) ? poster.GetString() : null,
                BackdropPath = series.TryGetProperty("backdrop_path", out var backdrop) ? backdrop.GetString() : null,
                FirstAirDate = series.TryGetProperty("first_air_date", out var firstAirDate) ? firstAirDate.GetString() : null,
                VoteAverage = series.TryGetProperty("vote_average", out var vote) ? vote.GetDouble(): 0,
                NumberOfSeasons = series.TryGetProperty("number_of_seasons", out var seasons) ? seasons.GetInt32() : 0,
                NumberOfEpisodes = series.TryGetProperty("number_of_episodes", out var episodes) ? episodes.GetInt32() : 0,
                Genres = genres,
                Creators = creators,
                Cast = cast,
                WatchProviders = watchProviders
            };
        }
    }
}
