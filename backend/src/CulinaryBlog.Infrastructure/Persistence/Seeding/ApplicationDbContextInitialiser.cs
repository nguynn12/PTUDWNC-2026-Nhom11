using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Persistence.Seeding;

/// <summary>
/// Áp dụng migration đang chờ (InitialiseAsync) + sinh dữ liệu mẫu ngẫu nhiên
/// (SeedAsync) cho môi trường phát triển. Theo mẫu hình Clean Architecture (Jason
/// Taylor template) — được gọi từ Program.cs lúc khởi động ứng dụng ở môi trường
/// Development, KHÔNG chạy ở Production.
///
/// PHẠM VI: chỉ sinh dữ liệu cho phần Auth/User/Phân quyền (nhiệm vụ được giao —
/// FR-AUTH-001..010). Sinh dữ liệu Category/Recipe (>=20 categories, >=100 recipes,
/// mỗi recipe >=10 nguyên liệu và >=5 bước chế biến) thuộc phạm vi của thành viên
/// phụ trách module Recipe — có thể viết seeder tương tự (Bogus) và gọi nối tiếp
/// SeedUsersAsync() bên dưới trong cùng SeedAsync(), hoặc trong initialiser riêng.
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
    /// Sinh người dùng mẫu ngẫu nhiên bằng thư viện Bogus — phục vụ kiểm thử đăng
    /// nhập/phân quyền (FR-AUTH-001..010) mà không cần tự tay đăng ký thủ công từng
    /// tài khoản. Dùng UserManager.CreateAsync (KHÔNG insert thẳng vào DbSet) để mật
    /// khẩu được PasswordHasher&lt;ApplicationUser&gt; của Identity hash đúng chuẩn —
    /// khớp policy đã cấu hình ở DependencyInjection.AddInfrastructure (tối thiểu 8 ký
    /// tự, có hoa/thường/số/ký tự đặc biệt).
    /// </summary>
    private async Task SeedUsersAsync()
    {
        const int sampleUserCount = 15;
        const string samplePassword = "Passw0rd!23";

        if (await context.Users.CountAsync() > 0)
        {
            logger.LogInformation("AspNetUsers đã có dữ liệu — bỏ qua bước seed user mẫu.");
            return;
        }

        var userFaker = new Faker<ApplicationUser>()
            .CustomInstantiator(f =>
            {
                var firstName = f.Name.FirstName();
                var lastName = f.Name.LastName();
                var displayName = $"{firstName} {lastName}";
                var email = f.Internet.Email(firstName, lastName).ToLowerInvariant();

                return ApplicationUser.Create(email, displayName);
            })
            .RuleFor(u => u.Bio, f => f.Lorem.Sentence(10))
            .RuleFor(u => u.AvatarUrl, f => f.Internet.Avatar())
            .RuleFor(u => u.EmailConfirmed, f => f.Random.Bool(0.7f));

        var sampleUsers = userFaker.Generate(sampleUserCount);

        foreach (var user in sampleUsers)
        {
            var result = await userManager.CreateAsync(user, samplePassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.Author);
            }
            else
            {
                logger.LogWarning(
                    "Không seed được user mẫu {Email}: {Errors}",
                    user.Email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation("Đã seed {Count} user mẫu (role Author, mật khẩu mẫu: {Password}) bằng Bogus.",
            sampleUserCount, samplePassword);
    }
}
