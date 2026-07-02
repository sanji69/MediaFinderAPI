using MediaFinder.Interface;
using MediaFinder.Options;
using Microsoft.Extensions.Options;

namespace MediaFinder.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly LocalizationOptions _localizationOptions;

        public LocalizationService(IOptions<LocalizationOptions> localizationOptions)
        {
            _localizationOptions = localizationOptions.Value;
        }
        public string ResolveLanguage(string? language)
        {
            return string.IsNullOrWhiteSpace(language)
                ? _localizationOptions.Language
                : language;
        }

        public string ResolveCountryCode(string? countryCode)
        {
            return string.IsNullOrWhiteSpace(countryCode)
                ? _localizationOptions.CountryCode
                : countryCode;
        }
    }
}
