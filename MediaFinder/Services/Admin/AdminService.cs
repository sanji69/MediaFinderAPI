using MediaFinder.Data;
using MediaFinder.DTOs.Admin;
using MediaFinder.Enums;
using MediaFinder.Interface;
using Microsoft.EntityFrameworkCore;

namespace MediaFinder.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly MediaFinderDbContext _dbContext;

        public AdminService(MediaFinderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AdminCommentReportDto>> GetPendingReportsAsync(Guid currentUserId)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);

            var query = _dbContext.CommentReports
                .AsNoTracking()
                .Include(x => x.Comment)
                    .ThenInclude(x => x.User)
                .Include(x => x.ReporterUser)
                .Where(x => x.Status == ReportStatus.Pending)
                .Where(x => x.Comment.UserId != currentUserId);

            if (currentUser.Role == UserRole.Moderator)
            {
                query = query.Where(x => x.Comment.User.Role == UserRole.User);
            }

            return await query
                .OrderBy(x => x.CreatedAt)
                .Select(x => new AdminCommentReportDto
                {
                    Id = x.Id,
                    CommentId = x.CommentId,
                    CommentContent = x.Comment.Content,
                    CommentAuthorUsername = x.Comment.User.Username,
                    CommentAuthorUserId = x.Comment.UserId,
                    CommentAuthorRole = x.Comment.User.Role,
                    CommentAuthorWarningCount = x.Comment.User.WarningCount,

                    ReporterUserId = x.ReporterUserId,
                    ReporterUsername = x.ReporterUser.Username,

                    Reason = x.Reason,
                    Status = x.Status,

                    MediaId = x.Comment.MediaId,
                    MediaType = x.Comment.MediaType,
                    MediaTitle = x.Comment.MediaTitle,

                    CreatedAt = x.CreatedAt,
                    ReviewedAt = x.ReviewedAt,
                    ReviewedByUserId = x.ReviewedByUserId
                })
                .ToListAsync();
        }

        public async Task RejectReportAsync(Guid currentUserId, Guid reportId)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);

            var report = await GetReportWithCommentAuthorAsync(reportId);

            EnsureCanModerateReport(currentUser, report);

            report.Status = ReportStatus.Rejected;
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewedByUserId = currentUserId;

            await _dbContext.SaveChangesAsync();
        }

        public async Task AcceptReportAsync(Guid currentUserId, Guid reportId)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);

            var report = await GetReportWithCommentAuthorAsync(reportId);

            EnsureCanModerateReport(currentUser, report);

            var comment = report.Comment;
            var author = comment.User;

            report.Status = ReportStatus.Accepted;
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewedByUserId = currentUserId;

            comment.Status = CommentStatus.Hidden;
            comment.HiddenAt = DateTime.UtcNow;
            comment.ModeratedByUserId = currentUserId;
            comment.UpdatedAt = DateTime.UtcNow;

            author.WarningCount++;
            author.UpdatedAt = DateTime.UtcNow;

            if (author.Role == UserRole.User && author.WarningCount >= 3)
            {
                author.AccountStatus = AccountStatus.Banned;
                author.BannedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<AdminUserDto>> GetUsersAsync(Guid currentUserId)
        {
            await GetCurrentModeratorAsync(currentUserId);

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.Id != currentUserId &&
                    x.AccountStatus != AccountStatus.Deleted)
                .OrderBy(x => x.Username)
                .Select(x => new AdminUserDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    AvatarPath = x.AvatarPath,
                    Role = x.Role,
                    AccountStatus = x.AccountStatus,
                    WarningCount = x.WarningCount,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    DeletedAt = x.DeletedAt,
                    BannedAt = x.BannedAt
                })
                .ToListAsync();
        }

        public async Task BanUserAsync(Guid currentUserId, Guid targetUserId)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);
            var targetUser = await GetTargetUserAsync(targetUserId);

            EnsureCanActOnUser(currentUser, targetUser);

            targetUser.AccountStatus = AccountStatus.Banned;
            targetUser.BannedAt = DateTime.UtcNow;
            targetUser.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task UnbanUserAsync(Guid currentUserId, Guid targetUserId)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);
            var targetUser = await GetTargetUserAsync(targetUserId);

            EnsureCanActOnUser(currentUser, targetUser);

            targetUser.AccountStatus = AccountStatus.Active;
            targetUser.WarningCount = 0;
            targetUser.BannedAt = null;
            targetUser.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetWarningsAsync(Guid currentUserId, Guid targetUserId)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);
            var targetUser = await GetTargetUserAsync(targetUserId);

            EnsureCanActOnUser(currentUser, targetUser);

            targetUser.WarningCount = 0;
            targetUser.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserRoleAsync(Guid currentUserId, Guid targetUserId, UserRole newRole)
        {
            var currentUser = await GetCurrentModeratorAsync(currentUserId);
            var targetUser = await GetTargetUserAsync(targetUserId);

            if (currentUser.Role != UserRole.Admin)
                throw new InvalidOperationException("ADMIN_INSUFFICIENT_PERMISSION");

            if (currentUser.Id == targetUser.Id)
                throw new InvalidOperationException("ADMIN_SELF_ACTION_FORBIDDEN");

            targetUser.Role = newRole;
            targetUser.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private async Task<Entities.User> GetCurrentModeratorAsync(Guid currentUserId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == currentUserId &&
                    x.AccountStatus == AccountStatus.Active);

            if (user == null)
                throw new InvalidOperationException("ADMIN_USER_NOT_FOUND");

            if (user.Role != UserRole.Admin && user.Role != UserRole.Moderator)
                throw new InvalidOperationException("ADMIN_INSUFFICIENT_PERMISSION");

            return user;
        }

        private async Task<Entities.User> GetTargetUserAsync(Guid targetUserId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == targetUserId);

            if (user == null)
                throw new InvalidOperationException("ADMIN_USER_NOT_FOUND");

            return user;
        }

        private async Task<Entities.CommentReport> GetReportWithCommentAuthorAsync(Guid reportId)
        {
            var report = await _dbContext.CommentReports
                .Include(x => x.Comment)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == reportId);

            if (report == null)
                throw new InvalidOperationException("ADMIN_REPORT_NOT_FOUND");

            if (report.Status != ReportStatus.Pending)
                throw new InvalidOperationException("ADMIN_REPORT_ALREADY_REVIEWED");

            return report;
        }

        private static void EnsureCanModerateReport(Entities.User currentUser, Entities.CommentReport report)
        {
            var commentAuthor = report.Comment.User;

            if (commentAuthor.Id == currentUser.Id)
                throw new InvalidOperationException("ADMIN_CANNOT_MODERATE_OWN_COMMENT");

            if (currentUser.Role == UserRole.Moderator && commentAuthor.Role != UserRole.User)
                throw new InvalidOperationException("ADMIN_INSUFFICIENT_PERMISSION");
        }

        private static void EnsureCanActOnUser(Entities.User currentUser, Entities.User targetUser)
        {
            if (currentUser.Id == targetUser.Id)
                throw new InvalidOperationException("ADMIN_SELF_ACTION_FORBIDDEN");

            if (currentUser.Role == UserRole.Moderator && targetUser.Role != UserRole.User)
                throw new InvalidOperationException("ADMIN_INSUFFICIENT_PERMISSION");
        }
    }
}
