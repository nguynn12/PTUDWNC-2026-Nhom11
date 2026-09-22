namespace CulinaryBlog.Infrastructure.Configurations;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình EF Core Fluent API cho thực thể RecipeSlugHistory (bảng RecipeSlugHistories).
/// Hỗ trợ truy vấn nhanh và chuyển hướng SEO 301 khi slug thay đổi.
/// </summary>
public class RecipeSlugHistoryConfiguration : IEntityTypeConfiguration<RecipeSlugHistory>
{
    public void Configure(EntityTypeBuilder<RecipeSlugHistory> builder)
    {
        // 1. Tên bảng
        builder.ToTable("RecipeSlugHistories");

        // 2. Khóa chính
        builder.HasKey(h => h.Id);

        // 3. Các thuộc tính
        builder.Property(h => h.RecipeId)
            .IsRequired();

        builder.Property(h => h.OldSlug)
            .HasMaxLength(160)
            .IsRequired();

        // 4. Các trường Audit & Soft Delete
        builder.Property(h => h.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(h => h.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        // 5. Concurrency Token qua PostgreSQL xmin
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // 6. Global Query Filter
        builder.HasQueryFilter(h => !h.IsDeleted);

        // 7. Indexes
        builder.HasIndex(h => h.OldSlug)
            .HasDatabaseName("IDX_RecipeSlugHistory_OldSlug");

        builder.HasIndex(h => h.RecipeId)
            .HasDatabaseName("IDX_RecipeSlugHistory_RecipeId");
    }
}
