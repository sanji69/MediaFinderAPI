using MediaFinder.Data;
using MediaFinder.DTOs.Ratings;
using MediaFinder.Entities;
using MediaFinder.Interface;
using Microsoft.EntityFrameworkCore;

namespace MediaFinder.Services.Ratings
{
    public class RatingService : IRatingService
    {
        private readonly MediaFinderDbContext _dbContext;

        public RatingService(MediaFinderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RatingSummaryDto> GetRatingSummaryAsync(int mediaId, string mediaType, Guid? userId = null)
        {
            var query = _dbContext.Ratings
                .Where(x => x.MediaId == mediaId && x.MediaType == mediaType);

            var voteCount = await query.CountAsync();

            var averageScore = voteCount > 0
                ? await query.AverageAsync(x => x.Score)
                : 0;

            decimal? currentUserScore = null;

            if (userId.HasValue)
            {
                currentUserScore = await query
                    .Where(x => x.UserId == userId.Value)
                    .Select(x => (decimal?)x.Score)
                    .FirstOrDefaultAsync();
            }

            return new RatingSummaryDto
            {
                MediaId = mediaId,
                MediaType = mediaType,
                AverageScore = averageScore,
                VoteCount = voteCount,
                CurrentUserScore = currentUserScore
            };
        }

        public async Task<RatingSummaryDto> UpsertRatingAsync(Guid userId, UpsertRatingRequestDto request)
        {
            if (request.Score < 0m || request.Score > 5m)
                throw new InvalidOperationException("Score must be between 0 and 5 inclusive.");

            var rating = await _dbContext.Ratings.FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.MediaId == request.MediaId &&
                x.MediaType == request.MediaType);

            if (rating == null)
            {
                rating = new Rating
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MediaId = request.MediaId,
                    MediaType = request.MediaType,
                    Score = request.Score,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Ratings.Add(rating);
            }
            else
            {
                rating.Score = request.Score;
                rating.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return await GetRatingSummaryAsync(request.MediaId, request.MediaType, userId);
        }
    }
}
