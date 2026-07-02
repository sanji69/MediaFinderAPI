using MediaFinder.Data;
using MediaFinder.DTOs.Favorites;
using MediaFinder.Entities;
using MediaFinder.Interface;
using Microsoft.EntityFrameworkCore;

namespace MediaFinder.Services.Favorites
{
    public class FavoriteService : IFavoriteService
    {
        private readonly MediaFinderDbContext _dbContext;

        public FavoriteService(MediaFinderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FavoriteDto>> GetFavoritesAsync(Guid userId)
        {
            return await _dbContext.Favorites
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new FavoriteDto
                {
                    Id = x.Id,
                    MediaId = x.MediaId,
                    MediaType = x.MediaType,
                    Title = x.Title,
                    PosterPath = x.PosterPath,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FavoriteStatusDto> GetFavoriteStatusAsync(Guid userId, int mediaId, string mediaType)
        {
            var exists = await _dbContext.Favorites.AnyAsync(x =>
                x.UserId == userId &&
                x.MediaId == mediaId &&
                x.MediaType == mediaType);

            return new FavoriteStatusDto
            {
                MediaId = mediaId,
                MediaType = mediaType,
                IsFavorite = exists
            };
        }

        public async Task<FavoriteStatusDto> AddFavoriteAsync(Guid userId, AddFavoriteRequestDto request)
        {
            var exists = await _dbContext.Favorites.AnyAsync(x =>
                x.UserId == userId &&
                x.MediaId == request.MediaId &&
                x.MediaType == request.MediaType);

            if (!exists)
            {
                _dbContext.Favorites.Add(new Favorite
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MediaId = request.MediaId,
                    MediaType = request.MediaType,
                    Title = request.Title,
                    PosterPath = request.PosterPath,
                    CreatedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync();
            }

            return new FavoriteStatusDto
            {
                MediaId = request.MediaId,
                MediaType = request.MediaType,
                IsFavorite = true
            };
        }

        public async Task<FavoriteStatusDto> RemoveFavoriteAsync(Guid userId, int mediaId, string mediaType)
        {
            var favorite = await _dbContext.Favorites.FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.MediaId == mediaId &&
                x.MediaType == mediaType);

            if (favorite != null)
            {
                _dbContext.Favorites.Remove(favorite);
                await _dbContext.SaveChangesAsync();
            }

            return new FavoriteStatusDto
            {
                MediaId = mediaId,
                MediaType = mediaType,
                IsFavorite = false
            };
        }
    }
}
