namespace MediaFinder.DTOs
{
    public class CastMemberDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Character { get; set; } = string.Empty;
        public string? ProfilePath { get; set; }
    }
}
