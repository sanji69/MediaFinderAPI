using MediaFinder.DTOs;

namespace MediaFinder.Interface
{
    public interface ISearchService
    {
        Task<List<SearchResultDto>> SearchMultiAsync(string title, string? language);
        Task<List<SearchResultDto>> SearchByPersonAsync(int personId, string role, string? language);
        Task<List<SearchResultDto>> SearchByGenreAsync( int genreId, string sourceMediaType, string? language = null);
    }
}
