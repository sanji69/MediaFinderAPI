namespace MediaFinder.Interface
{
    public interface IEbayAuthService
    {
        Task<string> GetAccessTokenAsync();
    }
}
