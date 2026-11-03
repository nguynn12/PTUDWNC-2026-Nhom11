namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Dịch vụ khởi tạo và xuất bản Sitemap XML (FR-JOB-003, NFR-SEO).
/// </summary>
public interface ISitemapService
{
    /// <summary>
    /// Sinh nội dung XML Sitemap chuẩn cho tất cả công thức Published, danh mục và trang tĩnh.
    /// Loại bỏ 100% công thức Draft, Archived hoặc đã xóa mềm.
    /// </summary>
    Task<string> GenerateSitemapXmlAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Thực thi job định kỳ hàng ngày (02:00 UTC) với cơ chế khóa phân tán RedLock (FR-JOB-003).
    /// Ghi file vào wwwroot/sitemap.xml và log số lượng URL được tạo.
    /// </summary>
    Task<int> ExecuteSitemapGenerationJobAsync(CancellationToken cancellationToken = default);
}

