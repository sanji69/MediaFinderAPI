using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediaFinder.DTOs.Comments;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{mediaType}/{mediaId:int}")]
        public async Task<ActionResult<List<CommentResponseDto>>> GetByMedia(string mediaType, int mediaId)
        {
            Guid? currentUserId = null;

            var userIdValue =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (Guid.TryParse(userIdValue, out var parsedId))
            {
                currentUserId = parsedId;
            }

            var comments = await _commentService.GetByMediaAsync(mediaType, mediaId, currentUserId);

            return Ok(comments);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<List<CommentResponseDto>>> GetMine()
        {
            var userId = GetCurrentUserId();

            var comments = await _commentService.GetMineAsync(userId);

            return Ok(comments);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentResponseDto>> Create(CreateCommentRequestDto request)
        {
            var userId = GetCurrentUserId();

            var comment = await _commentService.CreateAsync(userId, request);

            return CreatedAtAction(
                nameof(GetByMedia),
                new
                {
                    mediaType = comment.MediaType,
                    mediaId = comment.MediaId
                },
                comment);
        }

        [Authorize]
        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> Delete(Guid commentId)
        {
            var userId = GetCurrentUserId();

            await _commentService.DeleteAsync(userId, commentId);

            return NoContent();
        }

        [Authorize]
        [HttpPost("{commentId:guid}/report")]
        public async Task<IActionResult> Report(
            Guid commentId,
            CreateCommentReportRequestDto request)
        {
            var userId = GetCurrentUserId();

            try
            {
                await _commentService.ReportAsync(userId, commentId, request);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { code = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("Invalid user token.");

            return userId;
        }
    }
}
