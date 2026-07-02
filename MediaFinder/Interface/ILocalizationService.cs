namespace MediaFinder.Interface
{
    public interface ILocalizationService
    {
        string ResolveLanguage(string? language);
        string ResolveCountryCode(string? countryCode);
    }
}
