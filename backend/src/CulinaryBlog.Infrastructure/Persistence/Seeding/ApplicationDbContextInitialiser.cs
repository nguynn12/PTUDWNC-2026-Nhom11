using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Persistence.Seeding;

/// <summary>
/// Áp dụng migration đang chờ và tạo các role/tài khoản cố định phục vụ đăng nhập,
/// phân quyền và liên kết công thức JSON. Được gọi khi bật migrate/seed trong Program.cs;
/// không tự sinh người dùng ngẫu nhiên.
/// </summary>
public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    CulinaryBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    public async Task InitialiseAsync()
    {
        try
        {
            // Áp dụng các migration chưa chạy (Database.MigrateAsync tự tạo bảng
            // __EFMigrationsHistory nếu chưa có). An toàn khi chạy lại nhiều lần — nếu
            // migration InitialCreate đã được đánh dấu "applied" thủ công qua
            // backend/scripts/sql/002_mark_initial_migration_applied.sql thì bước này
            // sẽ không cố tạo lại các bảng đã tồn tại.
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi áp dụng migration cho CulinaryBlogDbContext.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi sinh dữ liệu mẫu cho CulinaryBlogDbContext.");
            throw;
        }
    }

    /// <summary>
    /// Phòng hờ — 2 role Admin/Author đã được seed sẵn qua migration HasData
    /// (RoleConfiguration.cs), hàm này chỉ đảm bảo idempotent nếu ai đó lỡ xoá role
    /// khỏi database dev.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { Roles.Admin, Roles.Author })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    /// <summary>
    /// Tài khoản cố định để liên kết đúng tác giả của các công thức JSON.
    /// </summary>
    private static readonly (string Email, string DisplayName)[] RecipeAuthors =
    [
        ("chef.thuha@culinaryblog.com", "Nguyễn Thu Hà"),
        ("chef.minhkhoi@culinaryblog.com", "Trần Minh Khôi"),
        ("chef.baongoc@culinaryblog.com", "Lê Bảo Ngọc"),
        ("chef.quocviet@culinaryblog.com", "Phạm Quốc Việt"),
    ];

    private async Task SeedUsersAsync()
    {
        const string samplePassword = "Passw0rd!23";

        // Tạo các tài khoản tĩnh dùng cho việc test hệ thống
        await CreateUserIfNotExists("admin@culinaryblog.com", "Admin User", samplePassword, Roles.Admin, true);
        await CreateUserIfNotExists("author@culinaryblog.com", "Author User", samplePassword, Roles.Author, true);
        await CreateUserIfNotExists("author.unverified@culinaryblog.com", "Unverified Author", samplePassword, Roles.Author, false);

        // Tác giả cố định cho công thức thật (backend/data/recipes): email ổn định để bộ nạp liên kết đúng tác giả.
        foreach (var (email, name) in RecipeAuthors)
            await CreateUserIfNotExists(email, name, samplePassword, Roles.Author, true);
    }

    private async Task CreateUserIfNotExists(string email, string displayName, string password, string role, bool emailConfirmed)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = ApplicationUser.Create(email, displayName);
            user.EmailConfirmed = emailConfirmed;
            // Ảnh hồ sơ mặc định (chữ cái đầu của tên) để tài khoản mẫu có ảnh như tài khoản thật.
            user.AvatarUrl = "https://ui-avatars.com/api/?name="
                + Uri.EscapeDataString(displayName).Replace("%20", "+")
                + "&size=256&background=fed8c4&color=c85a17&bold=true";
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("Đã seed tài khoản tĩnh {Email} ({Role})", email, role);
            }
            else
            {
                logger.LogWarning("Lỗi seed tài khoản tĩnh {Email}: {Errors}", email, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
