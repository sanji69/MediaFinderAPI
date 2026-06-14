using MediaFinder.DTOs.Ratings;

namespace MediaFinder.Interface
{
    public interface IRatingService
    {
        Task<RatingSummaryDto> GetRatingSummaryAsync(int mediaId, string mediaType, Guid? userId = null);
        Task<RatingSummaryDto> UpsertRatingAsync(Guid userId, UpsertRatingRequestDto request);
    }
}
