using System.Security.Claims;
using MediaFinder.DTOs.Favorites;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/favorites")]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<ActionResult<List<FavoriteDto>>> GetFavorites()
        {
            var userId = GetCurrentUserId();

            var result = await _favoriteService.GetFavoritesAsync(userId);

            return Ok(result);
        }

        [HttpGet("status/{mediaType}/{mediaId:int}")]
        public async Task<ActionResult<FavoriteStatusDto>> GetFavoriteStatus(string mediaType, int mediaId)
        {
            var userId = GetCurrentUserId();

            var result = await _favoriteService.GetFavoriteStatusAsync(
                userId,
                mediaId,
                mediaType);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<FavoriteStatusDto>> AddFavorite(AddFavoriteRequestDto request)
        {
            var userId = GetCurrentUserId();

            var result = await _favoriteService.AddFavoriteAsync(
                userId,
                request);

            return Ok(result);
        }

        [HttpDelete("{mediaType}/{mediaId:int}")]
        public async Task<ActionResult<FavoriteStatusDto>> RemoveFavorite(string mediaType, int mediaId)
        {
            var userId = GetCurrentUserId();

            var result = await _favoriteService.RemoveFavoriteAsync(
                userId,
                mediaId,
                mediaType);

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
    }
}
