using MediaFinder.DTOs.Admin;
using MediaFinder.Enums;

namespace MediaFinder.Interface
{
    public interface IAdminService
    {
        Task<List<AdminCommentReportDto>> GetPendingReportsAsync(Guid currentUserId);
        Task RejectReportAsync(Guid currentUserId, Guid reportId);
        Task AcceptReportAsync(Guid currentUserId, Guid reportId);
        Task<List<AdminUserDto>> GetUsersAsync(Guid currentUserId);
        Task BanUserAsync(Guid currentUserId, Guid targetUserId);
        Task UnbanUserAsync(Guid currentUserId, Guid targetUserId);
        Task ResetWarningsAsync(Guid currentUserId, Guid targetUserId);
        Task UpdateUserRoleAsync(Guid currentUserId, Guid targetUserId, UserRole newRole);
    }
}
