namespace CulinaryBlog.Application.Common.Models;

/// <summary>Kết quả tạo tài khoản (SRS FR-AUTH-001).</summary>
/// <param name="Succeeded">Tạo thành công.</param>
/// <param name="UserId">Id user mới (rỗng nếu thất bại).</param>
/// <param name="DuplicateEmail">Email đã tồn tại → 409 <c>AUTH_EMAIL_EXISTS</c>.</param>
/// <param name="Errors">Lỗi của Identity theo field (ví dụ <c>password</c>) → 422.</param>
public sealed record CreateUserResult(
    bool Succeeded,
    string UserId,
    bool DuplicateEmail,
    IReadOnlyDictionary<string, string[]> Errors)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    public static CreateUserResult Success(string userId) => new(true, userId, false, NoErrors);

    public static CreateUserResult Duplicate() => new(false, string.Empty, true, NoErrors);

    public static CreateUserResult Failed(IReadOnlyDictionary<string, string[]> errors) => new(false, string.Empty, false, errors);
}
