using MediaFinder.DTOs;
using MediaFinder.Interface;
using MediaFinder.Models.Tmdb;
using MediaFinder.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MediaFinder.Services.Tmdb
{
    public class SearchService : ISearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly TmdbOptions _options;

        public SearchService(HttpClient httpClient, ILocalizationService localizationService, IOptions<TmdbOptions> options)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _options = options.Value;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.BearerToken);
        }

        public async Task<List<SearchResultDto>> SearchMultiAsync( string query, string? language = null)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return new List<SearchResultDto>();

            var resolvedLanguage = _localizationService.ResolveLanguage(language);
            var encodedQuery = Uri.EscapeDataString(query.Trim());

            var response = await _httpClient.GetAsync(
                $"search/multi?language={resolvedLanguage}&query={encodedQuery}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            var results = document.RootElement.GetProperty("results");

            return results.EnumerateArray()
                .Where(x =>
                    x.TryGetProperty("media_type", out var mediaType)
                    && (
                        mediaType.GetString() == "movie"
                        || mediaType.GetString() == "tv"
                    ))
                .Where(x =>
                    x.TryGetProperty("poster_path", out var posterPath)
                    && posterPath.ValueKind != JsonValueKind.Null
                    && !string.IsNullOrWhiteSpace(posterPath.GetString()))
                .Select(x =>
                {
                    var mediaType = x.GetProperty("media_type").GetString() ?? string.Empty;

                    var title = mediaType == "movie"
                        ? x.GetProperty("title").GetString()
                        : x.GetProperty("name").GetString();

                    return new SearchResultDto
                    {
                        Id = x.GetProperty("id").GetInt32(),
                        MediaType = mediaType,
                        Title = title ?? string.Empty,
                        PosterPath = x.GetProperty("poster_path").GetString()
                    };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .GroupBy(x => new { x.Id, x.MediaType })
                .Select(g => g.First())
                .ToList();
        }

        public async Task<List<SearchResultDto>> SearchByPersonAsync(int personId, string role, string? language = null)
        {
            var resolvedLanguage = _localizationService.ResolveLanguage(language);

            var response = await _httpClient.GetFromJsonAsync<TmdbCombinedCreditsResponse>(
                $"person/{personId}/combined_credits?language={resolvedLanguage}");

            if (response == null)
                return new List<SearchResultDto>();

            IEnumerable<TmdbCombinedCreditItem> credits = role.ToLower() switch
            {
                "actor" => response.Cast,

                "director" => response.Crew
                    .Where(x => x.Job == "Director" || x.Job == "Creator"),

                _ => Enumerable.Empty<TmdbCombinedCreditItem>()
            };

            return credits
                .Where(x => x.Media_Type == "movie" || x.Media_Type == "tv")
                .Where(x => !string.IsNullOrWhiteSpace(x.Poster_Path))
                .GroupBy(x => new { x.Id, x.Media_Type })
                .Select(g => g.First())
                .Select(x => new SearchResultDto
                {
                    Id = x.Id,
                    MediaType = x.Media_Type!,
                    Title = x.Media_Type == "movie"
                        ? x.Title ?? string.Empty
                        : x.Name ?? string.Empty,
                    PosterPath = x.Poster_Path
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .ToList();
        }

        public async Task<List<SearchResultDto>> SearchByGenreAsync( int genreId,string sourceMediaType, string? language = null)
        {
            var resolvedLanguage = _localizationService.ResolveLanguage(language);

            var sourceMediaTypeLower = sourceMediaType.ToLower();

            if (sourceMediaTypeLower != "movie" && sourceMediaType.ToLower() != "tv")
                throw new ArgumentException("sourceMediaType must be either 'movie' or 'tv'.");

            var targetMediaType = sourceMediaTypeLower == "movie" ? "tv" : "movie";

            var results = new List<SearchResultDto>();

            var sourceResults = await DiscoverByGenreAsync(sourceMediaTypeLower, genreId, resolvedLanguage);

            results.AddRange(sourceResults);

            var equivalentGenreId = await FindEquivalentGenreIdAsync(genreId, sourceMediaTypeLower);

            if (equivalentGenreId.HasValue)
            {
                var targetResults = await DiscoverByGenreAsync(targetMediaType, equivalentGenreId.Value, resolvedLanguage);
                results.AddRange(targetResults);
            }

            return results.GroupBy(x => new {x.Id, x.MediaType})
                .Select(g => g.First()).ToList();

        }

        private async Task<List<GenreDto>> GetGenresAsync(string mediaType) 
        { 
            var response = await _httpClient.GetAsync($"genre/{mediaType}/list?language=en-US"); response.EnsureSuccessStatusCode(); 
            var json = await response.Content.ReadAsStringAsync(); using var document = JsonDocument.Parse(json); 
            return document.RootElement.GetProperty("genres").EnumerateArray()
                .Select(x => new GenreDto { Id = x.GetProperty("id").GetInt32(), Name = x.GetProperty("name").GetString() ?? string.Empty })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList(); 
        }

        private async Task<int?> FindEquivalentGenreIdAsync(int genreId, string mediaType)
        {
            var sourcetype = mediaType.ToLower();
            var targetType = sourcetype == "movie" ? "tv" : "movie";

            var sourceGenres = await GetGenresAsync(sourcetype);
            var targetGenres = await GetGenresAsync(targetType);

            var sourceGenre = sourceGenres.FirstOrDefault(g => g.Id == genreId);
            if (sourceGenre == null)
                return null;
            
            var targetGenre = targetGenres.FirstOrDefault(g => string.Equals(g.Name, sourceGenre.Name, StringComparison.OrdinalIgnoreCase));


            return targetGenre?.Id;
        }

        private async Task<List<SearchResultDto>> DiscoverByGenreAsync( string mediaType, int genreId, string language)
        {
            var response = await _httpClient.GetAsync(
                $"discover/{mediaType}?language={language}&with_genres={genreId}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            var results = document.RootElement.GetProperty("results");

            return results.EnumerateArray()
                .Where(x =>
                    x.TryGetProperty("poster_path", out var posterPath)
                    && posterPath.ValueKind != JsonValueKind.Null
                    && !string.IsNullOrWhiteSpace(posterPath.GetString()))
                .Select(x =>
                {
                    var title = mediaType == "movie"
                        ? x.GetProperty("title").GetString()
                        : x.GetProperty("name").GetString();

                    return new SearchResultDto
                    {
                        Id = x.GetProperty("id").GetInt32(),
                        MediaType = mediaType,
                        Title = title ?? string.Empty,
                        PosterPath = x.GetProperty("poster_path").GetString()
                    };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .ToList();
        }
    }
}
