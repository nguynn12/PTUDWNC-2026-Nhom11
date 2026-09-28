namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Dữ liệu đầu vào không hợp lệ → 422 <c>VALIDATION_ERROR</c> kèm danh sách lỗi theo từng
/// field (trường <c>errors</c> của Problem Details). ValidationBehaviour (pipeline MediatR)
/// sẽ throw exception này khi FluentValidation báo lỗi.
/// </summary>
public sealed class ValidationFailedException : AppException
{
    public const string DefaultMessage = "Dữ liệu gửi lên không hợp lệ.";

    public ValidationFailedException(IReadOnlyDictionary<string, string[]> errors, string message = DefaultMessage)
        : base(ErrorCodes.ValidationError, message, AppErrorKind.Validation)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    /// <summary>Key = tên field, value = các thông báo lỗi của field đó.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
