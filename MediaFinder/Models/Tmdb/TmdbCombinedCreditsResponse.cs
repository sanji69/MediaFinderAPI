namespace MediaFinder.Models.Tmdb
{
    public class TmdbCombinedCreditsResponse
    {
        public List<TmdbCombinedCreditItem> Cast { get; set; } = new();
        public List<TmdbCombinedCreditItem> Crew { get; set; } = new();
    }
}
