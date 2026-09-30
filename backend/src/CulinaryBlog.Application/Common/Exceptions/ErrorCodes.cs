namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Application Error Codes chuẩn hoá theo SRS Phụ lục B (đã áp dụng RESOLVED-CONFLICTS C3/C4/C6).
/// Giá trị được trả trong trường <c>type</c> của Problem Details để frontend switch theo mã.
/// Thêm mã mới: bổ sung vào Phụ lục B trước, sau đó mới thêm hằng số ở đây.
/// </summary>
public static class ErrorCodes
{
    // ── Chung ────────────────────────────────────────────────────────────────
    public const string MalformedRequest = "MALFORMED_REQUEST";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";

    /// <summary>
    /// Lỗi không xử lý được (500). CHƯA có trong Phụ lục B — bổ sung để mọi response lỗi
    /// đều có <c>type</c> dạng mã, frontend không phải xử lý riêng link RFC mặc định.
    /// </summary>
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";

    // ── Auth ─────────────────────────────────────────────────────────────────
    public const string AuthEmailExists = "AUTH_EMAIL_EXISTS";
    public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AuthTokenInvalid = "AUTH_TOKEN_INVALID";
    public const string AuthTokenExpired = "AUTH_TOKEN_EXPIRED";
    public const string AuthRefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
    public const string AuthRefreshTokenRevoked = "AUTH_REFRESH_TOKEN_REVOKED";
    public const string AuthGoogleTokenInvalid = "AUTH_GOOGLE_TOKEN_INVALID";
    public const string AuthAccountDisabled = "AUTH_ACCOUNT_DISABLED";

    /// <summary>423 — khoá tạm sau 5 lần đăng nhập sai (FR-AUTH-002 A2). CHƯA có trong Phụ lục B — đề nghị bổ sung.</summary>
    public const string AuthAccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AuthEmailNotConfirmed = "AUTH_EMAIL_NOT_CONFIRMED";

    // ── Recipe ───────────────────────────────────────────────────────────────
    public const string RecipeNotFound = "RECIPE_NOT_FOUND";
    public const string RecipeForbidden = "RECIPE_FORBIDDEN";
    public const string RecipePublishIncomplete = "RECIPE_PUBLISH_INCOMPLETE";
    public const string RecipeConcurrencyConflict = "RECIPE_CONCURRENCY_CONFLICT";
    public const string RecipeNotDeleted = "RECIPE_NOT_DELETED";
    public const string RecipeRestoreWindowExpired = "RECIPE_RESTORE_WINDOW_EXPIRED";

    // ── Category ─────────────────────────────────────────────────────────────
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";
    public const string CategoryDeleteHasRecipes = "CATEGORY_DELETE_HAS_RECIPES";

    // ── File ─────────────────────────────────────────────────────────────────
    public const string FileSizeExceeded = "FILE_SIZE_EXCEEDED";
    public const string FileMimeInvalid = "FILE_MIME_INVALID";
}
