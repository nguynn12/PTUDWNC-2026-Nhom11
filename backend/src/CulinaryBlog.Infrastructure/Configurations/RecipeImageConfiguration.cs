namespace CulinaryBlog.Infrastructure.Configurations;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình EF Core Fluent API cho thực thể RecipeImage (bảng RecipeImages).
/// Tuân thủ đặc tả SRS v1.2.0 (mục 7.5) và quyết định kiến trúc E6, E7.
/// Phụ trách: Thành viên 4.
/// </summary>
public class RecipeImageConfiguration : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(EntityTypeBuilder<RecipeImage> builder)
    {
        // 1. Tên bảng
        builder.ToTable("RecipeImages");

        // 2. Khóa chính
        builder.HasKey(img => img.Id);

        // 3. Các thuộc tính
        builder.Property(img => img.RecipeId)
            .IsRequired();

        builder.Property(img => img.OriginalUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(img => img.MediumUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(img => img.ThumbnailUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(img => img.AltText)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(img => img.IsPrimary)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(img => img.OrderIndex)
            .HasDefaultValue(0)
            .IsRequired();

        // 4. Audit & Soft Delete (kế thừa BaseEntity)
        builder.Property(img => img.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(img => img.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(img => img.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        // 5. Quan hệ với Recipe (Cascade Delete khi xóa Recipe vĩnh viễn)
        builder.HasOne(img => img.Recipe)
            .WithMany(r => r.Images)
            .HasForeignKey(img => img.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // 6. Global Query Filter cho Soft Delete
        builder.HasQueryFilter(img => !img.IsDeleted);

        // 7. Indexes phục vụ tìm kiếm hình ảnh của Recipe và lọc ảnh chính
        builder.HasIndex(img => img.RecipeId)
            .HasDatabaseName("IX_RecipeImages_RecipeId");

        builder.HasIndex(img => new { img.RecipeId, img.IsPrimary })
            .HasDatabaseName("IX_RecipeImages_RecipeId_IsPrimary");

        builder.HasIndex(img => new { img.RecipeId, img.OrderIndex })
            .HasDatabaseName("IX_RecipeImages_RecipeId_OrderIndex");
    }
}
