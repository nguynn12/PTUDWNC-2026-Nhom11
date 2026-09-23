namespace CulinaryBlog.Infrastructure.Configurations;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình EF Core Fluent API cho thực thể Category (bảng Categories)
/// Tuân thủ chuẩn đặc tả SRS v1.2.0 (mục 7.1, 7.6 và FR-CAT).
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // 1. Tên bảng
        builder.ToTable("Categories");

        // 2. Khóa chính
        builder.HasKey(c => c.Id);

        // 3. Các thuộc tính
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.OrderIndex)
            .HasDefaultValue(0)
            .IsRequired();

        // 4. Các trường Audit & Soft Delete (theo mục 7.1 SRS v1.2.0)
        builder.Property(c => c.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.DeletedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        // 5. Concurrency Token qua PostgreSQL xmin (mục 7.1 SRS v1.2.0)
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // 6. Global Query Filter cho Soft Delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        // 7. Partial Unique Indexes khi IsDeleted = false (mục 7.6 SRS v1.2.0)
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IDX_Category_Name_Active");

        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IDX_Category_Slug");
    }
}
