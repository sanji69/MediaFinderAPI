using Azure;
using MediaFinder.Data;
using MediaFinder.DTOs.Comments;
using MediaFinder.Entities;
using MediaFinder.Enums;
using MediaFinder.Interface;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediaFinder.Services.Comments
{
    public class CommentService : ICommentService
    {
        private readonly MediaFinderDbContext _dbContext;

        public CommentService(MediaFinderDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<CommentResponseDto>> GetByMediaAsync(string mediaType, int mediaId, Guid? currentUserId = null)
        {
            var comments = await _dbContext.Comments
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Reports)
                .Where(x =>
                    x.MediaType == mediaType &&
                    x.MediaId == mediaId &&
                    x.Status == CommentStatus.Visible)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return comments.Select(x =>
            {
                var dto = ToDto(x);

                dto.HasCurrentUserReported =
                    currentUserId.HasValue &&
                    x.Reports.Any(r => r.ReporterUserId == currentUserId.Value);

                return dto;
            }).ToList();
        }

        public async Task<List<CommentResponseDto>> GetMineAsync(Guid userId)
        {
            return await _dbContext.Comments
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.UserId == userId &&
                    x.Status != CommentStatus.Deleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => ToDto(x))
                .ToListAsync();
        }

        public async Task<CommentResponseDto> CreateAsync(Guid userId, CreateCommentRequestDto request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.AccountStatus == AccountStatus.Active);

            if (user == null)
                throw new UnauthorizedAccessException("User not found or inactive.");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MediaId = request.MediaId,
                MediaType = request.MediaType.Trim().ToLowerInvariant(),
                MediaTitle = request.MediaTitle.Trim(),
                MediaPosterPath = request.MediaPosterPath,
                Content = request.Content.Trim(),
                Status = CommentStatus.Visible,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            comment.User = user;

            return ToDto(comment);
        }

        public async Task DeleteAsync(Guid userId, Guid commentId)
        {
            var comment = await _dbContext.Comments
                .FirstOrDefaultAsync(x => x.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found.");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You cannot delete this comment.");

            if (comment.Status == CommentStatus.Deleted)
                return;

            comment.Status = CommentStatus.Deleted;
            comment.DeletedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task ReportAsync(
            Guid userId,
            Guid commentId,
            CreateCommentReportRequestDto request)
        {
            var comment = await _dbContext.Comments
                .FirstOrDefaultAsync(x => x.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("COMMENT_NOT_FOUND");

            if (comment.Status != CommentStatus.Visible)
                throw new InvalidOperationException("COMMENT_NOT_AVAILABLE");

            if (comment.UserId == userId)
                throw new InvalidOperationException("COMMENT_REPORT_OWN_COMMENT");

            var alreadyReported = await _dbContext.CommentReports
                .AnyAsync(x =>
                    x.CommentId == commentId &&
                    x.ReporterUserId == userId);

            if (alreadyReported)
                throw new InvalidOperationException("COMMENT_ALREADY_REPORTED");

            var report = new CommentReport
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                ReporterUserId = userId,
                Reason = request.Reason.Trim(),
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.CommentReports.Add(report);
            await _dbContext.SaveChangesAsync();
        }

        private static CommentResponseDto ToDto(Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                Username = comment.User.Username,
                UserAvatarPath = comment.User.AvatarPath,
                MediaId = comment.MediaId,
                MediaType = comment.MediaType,
                MediaTitle = comment.MediaTitle,
                MediaPosterPath = comment.MediaPosterPath,
                Content = comment.Content,
                Status = comment.Status,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }
    }
}
