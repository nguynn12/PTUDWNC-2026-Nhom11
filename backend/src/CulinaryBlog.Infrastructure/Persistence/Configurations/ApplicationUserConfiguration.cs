using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình bổ sung cho ApplicationUser (bảng AspNetUsers). Các cột chuẩn của Identity
/// (Email, UserName, PasswordHash, SecurityStamp, LockoutEnd, AccessFailedCount, ...)
/// đã được cấu hình sẵn bởi IdentityDbContext + AddEntityFrameworkStores — ở đây chỉ
/// khai báo thêm các field tuỳ biến theo SRS.md mục 7.7.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Khớp varchar(450) với FK RefreshTokens.UserId / SRS.md mục 7.2 (AuthorId FK AspNetUsers).
        // Identity mặc định KHÔNG set MaxLength cho Id (map thành "text" nếu bỏ qua dòng này).
        builder.Property(u => u.Id)
            .HasMaxLength(450);

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasColumnType("text");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Truy vấn phổ biến của Admin: lọc danh sách user theo trạng thái active/inactive
        builder.HasIndex(u => u.IsActive);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
