namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Giao diện dịch vụ xóa vĩnh viễn (Purge) công thức và dọn rác tự động (FR-JOB-004, SRS Lab 8).
/// Dùng chung giữa thao tác thủ công của Quản trị viên và tiến trình Hangfire chạy ngầm.
/// </summary>
public interface IRecipePurgeService
{
    /// <summary>
    /// Xóa vĩnh viễn (xóa vật lý) một công thức đã bị xóa mềm, dọn dẹp các tệp ảnh đính kèm.
    /// </summary>
    Task<bool> PurgeRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Quét và xóa vĩnh viễn toàn bộ các công thức đã nằm trong thùng rác quá số ngày quy định (mặc định 30 ngày).
    /// </summary>
    Task<int> PurgeExpiredRecipesAsync(int daysThreshold = 30, CancellationToken cancellationToken = default);
}

