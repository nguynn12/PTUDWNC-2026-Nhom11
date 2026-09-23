namespace CulinaryBlog.Infrastructure.Configurations;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình EF Core Fluent API cho thực thể RecipeIngredient (bảng RecipeIngredients).
/// Tuân thủ đặc tả SRS v1.2.0 (mục 7.4) và quyết định kiến trúc E1, E2.
/// Phụ trách: Thành viên 4.
/// </summary>
public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        // 1. Tên bảng
        builder.ToTable("RecipeIngredients");

        // 2. Khóa chính
        builder.HasKey(i => i.Id);

        // 3. Các thuộc tính
        builder.Property(i => i.RecipeId)
            .IsRequired();

        builder.Property(i => i.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasPrecision(10, 3)
            .IsRequired(false);

        builder.Property(i => i.Unit)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(i => i.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(i => i.OrderIndex)
            .HasDefaultValue(0)
            .IsRequired();

        // 4. Audit & Soft Delete (kế thừa BaseEntity)
        builder.Property(i => i.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(i => i.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        // 5. Quan hệ với Recipe (Cascade Delete khi xóa Recipe vĩnh viễn)
        builder.HasOne(i => i.Recipe)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // 6. Global Query Filter cho Soft Delete
        builder.HasQueryFilter(i => !i.IsDeleted);

        // 7. Chỉ mục (Indexes)
        builder.HasIndex(i => i.RecipeId)
            .HasDatabaseName("IX_RecipeIngredients_RecipeId");

        builder.HasIndex(i => new { i.RecipeId, i.OrderIndex })
            .HasDatabaseName("IX_RecipeIngredients_RecipeId_OrderIndex");
    }
}
