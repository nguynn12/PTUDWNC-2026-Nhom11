namespace CulinaryBlog.API.ErrorHandling;

/// <summary>
/// Thông tin lỗi đã chuẩn hoá, sẵn sàng ghi ra RFC 7807 Problem Details.
/// </summary>
/// <param name="Status">HTTP status theo SRS Phụ lục A.</param>
/// <param name="ErrorCode">Application Error Code (SRS Phụ lục B) — ghi vào trường <c>type</c>.</param>
/// <param name="Title">Tiêu đề ngắn của nhóm lỗi.</param>
/// <param name="Detail">Mô tả cụ thể, an toàn để hiển thị cho người dùng.</param>
/// <param name="Errors">Lỗi theo từng field (chỉ có với lỗi validation).</param>
public sealed record ProblemDescriptor(
    int Status,
    string ErrorCode,
    string Title,
    string? Detail,
    IReadOnlyDictionary<string, string[]>? Errors = null);
