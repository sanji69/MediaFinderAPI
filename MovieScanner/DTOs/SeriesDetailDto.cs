namespace MediaFinder.DTOs
{
    public class SeriesDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public string? BackdropPath { get; set; }
        public string? FirstAirDate { get; set; }
        public double VoteAverage { get; set; }
        public int NumberOfSeasons { get; set; }
        public int NumberOfEpisodes { get; set; }
        public List<GenreDto> Genres { get; set; } = []; 
        public List<PersonDto> Creators { get; set; } = [];
        public List<CastMemberDto> Cast { get; set; } = [];
        public List<WatchProviderDto> WatchProviders { get; set; } = [];
    }
}
