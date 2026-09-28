namespace CulinaryBlog.Application.Features.RecipeSteps.DTOs;

/// <summary>
/// Hợp đồng dữ liệu yêu cầu sắp xếp lại thứ tự các bước thực hiện của công thức.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010).
/// </summary>
public class ReorderRecipeStepsRequest
{
    /// <summary>
    /// Danh sách mã định danh các bước theo thứ tự mới mong muốn.
    /// Phải chứa đầy đủ và chính xác tất cả các bước đang hoạt động của công thức.
    /// </summary>
    public List<Guid> StepIds { get; set; } = new();
}
