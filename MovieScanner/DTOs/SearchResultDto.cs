namespace MediaFinder.DTOs
{
    public class SearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? PosterPath { get; set; }
        public string MediaType { get; set; } = "";
    }
}
