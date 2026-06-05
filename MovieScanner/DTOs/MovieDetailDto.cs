namespace MediaFinder.DTOs
{
    public class MovieDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public string? BackdropPath { get; set; }
        public string? ReleaseDate { get; set; }
        public double VoteAverage { get; set; }
        public int Runtime { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
    }
}
