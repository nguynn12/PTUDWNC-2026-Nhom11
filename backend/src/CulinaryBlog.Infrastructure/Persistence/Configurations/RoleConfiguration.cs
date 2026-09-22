using CulinaryBlog.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Seed 2 role cố định: Author (gán tự động khi đăng ký) và Admin (gán thủ công qua
/// database seeding — SRS.md mục 2.3). GUID cố định để migration idempotent — KHÔNG đổi
/// giá trị sau khi đã deploy lên môi trường có dữ liệu thật.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public static readonly Guid AdminRoleId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid AuthorRoleId = Guid.Parse("20000000-0000-0000-0000-000000000002");

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        // Khớp varchar(450) với AspNetUsers.Id và các FK RoleId liên quan.
        builder.Property(r => r.Id)
            .HasMaxLength(450);

        builder.HasData(
            new IdentityRole
            {
                Id = AdminRoleId.ToString(),
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = "b9f6e7d1-0000-0000-0000-000000000001",
            },
            new IdentityRole
            {
                Id = AuthorRoleId.ToString(),
                Name = Roles.Author,
                NormalizedName = Roles.Author.ToUpperInvariant(),
                ConcurrencyStamp = "b9f6e7d1-0000-0000-0000-000000000002",
            });
    }
}
