using MediaFinder.DTOs.Favorites;

namespace MediaFinder.Interface
{
    public interface IFavoriteService
    {
        Task<FavoriteStatusDto> GetFavoriteStatusAsync(Guid userId, int mediaId, string mediaType);
        Task<FavoriteStatusDto> AddFavoriteAsync(Guid userId, AddFavoriteRequestDto request);
        Task<FavoriteStatusDto> RemoveFavoriteAsync(Guid userId, int mediaId, string mediaType);
        Task<List<FavoriteDto>> GetFavoritesAsync(Guid userId);
    }
}
