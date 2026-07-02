using System.Security.Claims;
using MediaFinder.DTOs.Ratings;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("{mediaType}/{mediaId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RatingSummaryDto>> GetRatingSummary(string mediaType, int mediaId)
        {
            var userId = GetCurrentUserIdOrNull();

            var result = await _ratingService.GetRatingSummaryAsync(
                mediaId,
                mediaType,
                userId);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RatingSummaryDto>> UpsertRating(UpsertRatingRequestDto request)
        {
            var userId = GetCurrentUserId();

            var result = await _ratingService.UpsertRatingAsync(
                userId,
                request);

            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            return Guid.Parse(userId);
        }

        private Guid? GetCurrentUserIdOrNull()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            return Guid.TryParse(userId, out var parsed)
                ? parsed
                : null;
        }
    }
}
