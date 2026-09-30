namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Lớp cơ sở cho tất cả ngoại lệ nghiệp vụ thuộc tầng Domain.
/// </summary>
public abstract class DomainException(string message, string errorCode = "DOMAIN_ERROR") : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

/// <summary>
/// Ngoại lệ ném ra khi không tìm thấy thực thể trong Domain.
/// </summary>
public class EntityNotFoundException(string name, object key, string errorCode = "RECIPE_NOT_FOUND")
    : DomainException($"Không tìm thấy thực thể \"{name}\" với mã định danh ({key}).", errorCode);

/// <summary>
/// Ngoại lệ ném ra khi vi phạm quyền sở hữu tài nguyên Domain (Author/Admin).
/// </summary>
public class OwnershipViolationException(string message = "Bạn không có quyền thực hiện thao tác trên tài nguyên này.", string errorCode = "RECIPE_FORBIDDEN")
    : DomainException(message, errorCode);

/// <summary>
/// Ngoại lệ ném ra khi vi phạm quy tắc chuyển đổi trạng thái của Recipe.
/// </summary>
public class InvalidRecipeStateTransitionException(string message, string errorCode = "RECIPE_INVALID_STATUS_TRANSITION")
    : DomainException(message, errorCode);

/// <summary>
/// Ngoại lệ ném ra khi điều kiện xuất bản công thức chưa hoàn tất (thiếu nguyên liệu hoặc bước nấu theo C4, C5).
/// </summary>
public class RecipeIncompletePublishException(string message = "Công thức cần có ít nhất 1 nguyên liệu và 1 bước thực hiện để được xuất bản.", string errorCode = "RECIPE_PUBLISH_INCOMPLETE")
    : DomainException(message, errorCode);
