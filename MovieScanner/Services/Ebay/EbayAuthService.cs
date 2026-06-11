using MediaFinder.Interface;
using MediaFinder.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MediaFinder.Services.Ebay
{
    public class EbayAuthService : IEbayAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly EbayOptions _options;

        private string? _accessToken;
        private DateTime _expiresAtUtc;

        public EbayAuthService(
            HttpClient httpClient,
            IOptions<EbayOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }



        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_accessToken)
                && DateTime.UtcNow < _expiresAtUtc)
            {
                return _accessToken;
            }

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                _options.OAuthUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = "https://api.ebay.com/oauth/api_scope"
            });

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            _accessToken = document.RootElement
                .GetProperty("access_token")
                .GetString();

            var expiresIn = document.RootElement
                .GetProperty("expires_in")
                .GetInt32();

            _expiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken!;
        }
    }
}
