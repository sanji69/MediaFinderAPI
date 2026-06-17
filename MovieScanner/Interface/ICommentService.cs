using MediaFinder.DTOs.Comments;

namespace MediaFinder.Interface
{
    public interface ICommentService
    {
        Task<List<CommentResponseDto>> GetByMediaAsync(string mediaType, int mediaId, Guid? currentUserId = null);
        Task<List<CommentResponseDto>> GetMineAsync(Guid userId);
        Task<CommentResponseDto> CreateAsync(Guid userId, CreateCommentRequestDto request);
        Task DeleteAsync(Guid userId, Guid commentId);
        Task ReportAsync(Guid userId, Guid commentId, CreateCommentReportRequestDto request);
    }
}
