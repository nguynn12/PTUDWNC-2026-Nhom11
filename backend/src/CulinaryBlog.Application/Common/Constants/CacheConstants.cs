namespace CulinaryBlog.Application.Common.Constants;

/// <summary>
/// Các hằng số thời gian hết hạn (TTL) và tiền tố khóa cache theo đặc tả NFR-PERF (SRS mục 5.1).
/// </summary>
public static class CacheConstants
{
    // Thời lượng TTL
    public static readonly TimeSpan CategoryDuration = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan RecipeListDuration = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan RecipeDetailDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan SearchDuration = TimeSpan.FromMinutes(1);

    // Tiền tố khóa (Prefix) phục vụ invalidate theo nhóm
    public const string CategoryPrefix = "categories:";
    public const string CategoryAllKey = "categories:all";

    public const string RecipePrefix = "recipes:";
    public const string RecipeListPrefix = "recipes:list:";
    public const string RecipeDetailPrefix = "recipes:id:";
    public const string RecipeSlugPrefix = "recipes:slug:";

    public const string SearchPrefix = "search:";

    // Khóa Distributed Lock RedLock
    public const string SitemapLockKey = "locks:sitemap-generation";
}

