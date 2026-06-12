using MediaFinder.DTOs.Offers;
using MediaFinder.Interface;
using MediaFinder.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Globalization;

namespace MediaFinder.Services.Ebay
{
    public class EbayOfferProvider : IPhysicalOfferProvider
    {
        private readonly HttpClient _httpClient;
        private readonly EbayOptions _options;
        private readonly IEbayAuthService _authService;
        private readonly ILocalizationService _localizationService;

        public EbayOfferProvider(HttpClient httpClient, IOptions<EbayOptions> options, IEbayAuthService authService, ILocalizationService localizationService)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _authService = authService;
            _localizationService = localizationService;

            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        public async Task<List<PhysicalOfferDto>> SearchAsync(PhysicalOfferSearchQuery query)
        {
            var countryCode = _localizationService.ResolveCountryCode(query.CountryCode);
            var marketplaceId = ResolveMarketplaceId(countryCode);

            var accessToken = await _authService.GetAccessTokenAsync();

            var searchQuery = BuildSearchQuery(query);
            var encodedQuery = Uri.EscapeDataString(searchQuery);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"item_summary/search?q={encodedQuery}&limit=10");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Add("X-EBAY-C-MARKETPLACE-ID", marketplaceId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("itemSummaries", out var items))
                return new List<PhysicalOfferDto>();

            return items.EnumerateArray()
                .Select(item => new PhysicalOfferDto
                {
                    Provider = "eBay",
                    Title = item.GetProperty("title").GetString() ?? string.Empty,
                    Url = item.GetProperty("itemWebUrl").GetString() ?? string.Empty,
                    ImageUrl = item.TryGetProperty("image", out var image)
                        && image.TryGetProperty("imageUrl", out var imageUrl)
                            ? imageUrl.GetString()
                            : null,
                    Price = item.TryGetProperty("price", out var price)
                        && price.TryGetProperty("value", out var value)
                        && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPrice)
                            ? parsedPrice
                            : 0,
                    Currency = item.TryGetProperty("price", out var priceCurrency)
                        && priceCurrency.TryGetProperty("currency", out var currency)
                            ? currency.GetString() ?? string.Empty
                            : string.Empty,
                    Condition = item.TryGetProperty("condition", out var condition)
                        ? condition.GetString()
                        : null,
                    Format = DetectFormat(item.GetProperty("title").GetString() ?? string.Empty),
                    OfferType = DetectOfferType(item.GetProperty("title").GetString() ?? string.Empty)
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .ToList();
        }

        private string ResolveMarketplaceId(string countryCode)
        {
            return countryCode.ToUpperInvariant() switch
            {
                "FR" => "EBAY_FR",
                "US" => "EBAY_US",
                _ => "EBAY_US"
            };
        }

        private string BuildSearchQuery(PhysicalOfferSearchQuery query)
        {
            if (query.MediaType == "tv" && query.SeasonNumber.HasValue)
            {
                return $"{query.Title} saison {query.SeasonNumber.Value} DVD Blu-ray";
            }

            if (query.MediaType == "tv")
            {
                return $"{query.Title} intégrale DVD Blu-ray";
            }

            return $"\"{query.Title}\" DVD Blu-ray";
        }

        private string? DetectFormat(string title)
        {
            var lower = title.ToLowerInvariant();

            if (lower.Contains("4k") || lower.Contains("uhd"))
                return "4K UHD";

            if (lower.Contains("blu-ray") || lower.Contains("bluray"))
                return "Blu-ray";

            if (lower.Contains("dvd"))
                return "DVD";

            return null;
        }

        private string DetectOfferType(string title)
        {
            var lower = title.ToLowerInvariant();

            if (lower.Contains("intégrale")
                || lower.Contains("integrale")
                || lower.Contains("complete series"))
                return "CompleteSeries";

            if (lower.Contains("coffret")
                || lower.Contains("box set"))
                return "BoxSet";

            if (lower.Contains("saison")
                || lower.Contains("season"))
                return "Season";

            return "Movie";
        }
    }
}
