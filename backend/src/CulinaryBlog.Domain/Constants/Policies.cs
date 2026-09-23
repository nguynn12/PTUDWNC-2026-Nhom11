namespace CulinaryBlog.Domain.Constants;

/// <summary>Tên các Authorization Policy dùng trong ASP.NET Core Authorization.</summary>
public static class Policies
{
    /// <summary>
    /// Yêu cầu email đã xác nhận (EmailConfirmed = true) hoặc role Admin.
    /// Áp dụng khi Publish Recipe — SRS mục 2.3: "Policy 'VerifiedAuthor' yêu cầu email đã
    /// xác nhận... Admin có quyền bypass." Xem thêm FR-AUTH-008.
    /// </summary>
    public const string VerifiedAuthor = "VerifiedAuthor";
}
