namespace CulinaryBlog.Infrastructure.Seeders;

using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Lớp khởi tạo dữ liệu mẫu ban đầu cho bảng Categories (Danh mục món ăn).
/// Đảm bảo hệ thống luôn có sẵn 8 danh mục nền tảng cho việc phân loại công thức.
/// </summary>
public static class CategorySeeder
{
    /// <summary>
    /// Danh sách 8 danh mục ẩm thực mẫu với UUID cố định (Deterministic GUID)
    /// để các thành viên khác có thể tham chiếu ID trong quá trình phát triển/test.
    /// </summary>
    public static readonly IReadOnlyList<Category> InitialCategories = new List<Category>
    {
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Món khai vị",
            Slug = "mon-khai-vi",
            Description = "Các món ăn nhẹ, gỏi, salad giúp kích thích vị giác mở đầu bữa ăn.",
            ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=800",
            OrderIndex = 1,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Món chính",
            Slug = "mon-chinh",
            Description = "Các món ăn giàu đạm, dinh dưỡng chính trong bữa cơm như món kho, xào, chiên, nướng.",
            ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=800",
            OrderIndex = 2,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Món canh / súp",
            Slug = "mon-canh-sup",
            Description = "Các món canh thanh nhiệt, súp bổ dưỡng thơm ngon cho cả gia đình.",
            ImageUrl = "https://images.unsplash.com/photo-1547592166-23ac45744acd?w=800",
            OrderIndex = 3,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Món tráng miệng",
            Slug = "mon-trang-mieng",
            Description = "Chè, hoa quả tươi, kem, pudding và các món ngọt kết thúc bữa ăn hoàn hảo.",
            ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=800",
            OrderIndex = 4,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Name = "Đồ uống & Trà",
            Slug = "do-uong-tra",
            Description = "Nước ép hoa quả, sinh tố, trà thảo mộc giải nhiệt và thức uống bồi bổ.",
            ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=800",
            OrderIndex = 5,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Name = "Món ăn vặt",
            Slug = "mon-an-vat",
            Description = "Các món ăn chơi đường phố, bánh tráng, đồ chiên rán hấp dẫn mọi lứa tuổi.",
            ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800",
            OrderIndex = 6,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Name = "Món chay",
            Slug = "mon-chay",
            Description = "Các món ăn thanh đạm, thuần thực vật, giàu chất xơ và tốt cho sức khỏe.",
            ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800",
            OrderIndex = 7,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        },
        new()
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Name = "Bánh & Điểm tâm",
            Slug = "banh-diem-tam",
            Description = "Bánh ngọt, bánh mì, bánh bao và các món điểm tâm sáng dinh dưỡng.",
            ImageUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=800",
            OrderIndex = 8,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        }
    };

    /// <summary>
    /// Thực thi nạp dữ liệu mẫu vào database nếu bảng Categories chưa có bản ghi nào.
    /// Phương thức an toàn (Idempotent), có thể gọi nhiều lần mà không sợ trùng lặp.
    /// </summary>
    /// <param name="context">DbContext của ứng dụng</param>
    public static async Task SeedAsync(CulinaryBlogDbContext context)
    {
        // Chỉ thêm dữ liệu nếu bảng Categories hiện chưa có bản ghi nào
        if (!await context.Categories.AnyAsync())
        {
            await context.Categories.AddRangeAsync(InitialCategories);
            await context.SaveChangesAsync();
        }
    }
}
