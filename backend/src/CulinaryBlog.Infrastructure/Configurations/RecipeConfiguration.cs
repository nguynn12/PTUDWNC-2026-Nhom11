namespace CulinaryBlog.Infrastructure.Configurations;

using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình EF Core Fluent API cho thực thể Recipe (bảng Recipes).
/// Tuân thủ chuẩn đặc tả SRS v1.2.0 (mục 7.1, 7.2) và các quyết định kiến trúc RESOLVED-CONFLICTS.md.
/// </summary>
public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        // 1. Tên bảng
        builder.ToTable("Recipes");

        // 2. Khóa chính
        builder.HasKey(r => r.Id);

        // 3. Các thuộc tính cơ bản
        builder.Property(r => r.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Slug)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnType("text")
            .IsRequired(true);

        builder.Property(r => r.PrepTimeMinutes)
            .IsRequired();

        builder.Property(r => r.CookTimeMinutes)
            .IsRequired();

        builder.Property(r => r.Servings)
            .IsRequired();

        builder.Property(r => r.Difficulty)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<short>()
            .HasDefaultValue(RecipeStatus.Draft)
            .IsRequired();

        // 4. Khóa ngoại liên kết với Category và ApplicationUser
        builder.Property(r => r.CategoryId)
            .IsRequired();

        builder.Property(r => r.AuthorId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Recipe (Domain) không có navigation Author — khai báo FK không navigation để giữ
        // nguyên ràng buộc AuthorId -> AspNetUsers.Id trong DB (RESOLVED-CONFLICTS.md mục D7).
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // 5. Các mốc thời gian xuất bản & xóa mềm
        builder.Property(r => r.PublishedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(r => r.DeletedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        // 6. Các trường Audit & Soft Delete (theo BaseEntity và mục 7.1 SRS)
        builder.Property(r => r.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        // 7. Concurrency Token qua PostgreSQL xmin (mục 7.1 SRS và RESOLVED-CONFLICTS C6)
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // 8. Cấu hình Owned Entity cho thông tin Dinh dưỡng (RESOLVED-CONFLICTS E3)
        builder.OwnsOne(r => r.Nutrition, nutrition =>
        {
            nutrition.Property(n => n.Calories)
                .HasPrecision(8, 2)
                .HasColumnName("Nutrition_Calories")
                .IsRequired(false);

            nutrition.Property(n => n.Protein)
                .HasPrecision(8, 2)
                .HasColumnName("Nutrition_Protein")
                .IsRequired(false);

            nutrition.Property(n => n.Carbohydrates)
                .HasPrecision(8, 2)
                .HasColumnName("Nutrition_Carbohydrates")
                .IsRequired(false);

            nutrition.Property(n => n.Fat)
                .HasPrecision(8, 2)
                .HasColumnName("Nutrition_Fat")
                .IsRequired(false);

            nutrition.Property(n => n.Fiber)
                .HasPrecision(8, 2)
                .HasColumnName("Nutrition_Fiber")
                .IsRequired(false);

            nutrition.Property(n => n.Sodium)
                .HasPrecision(8, 2)
                .HasColumnName("Nutrition_Sodium")
                .IsRequired(false);

            nutrition.Property(n => n.Source)
                .HasConversion<short>()
                .HasColumnName("Nutrition_Source")
                .HasDefaultValue(NutritionSource.Manual)
                .IsRequired();
        });

        // 9. Global Query Filter cho Soft Delete (RESOLVED-CONFLICTS A1)
        builder.HasQueryFilter(r => !r.IsDeleted);

        // 10. Partial Unique Index cho Slug khi IsDeleted = false (mục 7.2 SRS)
        builder.HasIndex(r => r.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IDX_Recipe_Slug");

        // 11. Composite Indexes phục vụ truy vấn, lọc và phân trang hiệu năng cao
        builder.HasIndex(r => new { r.Status, r.PublishedAt })
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IDX_Recipe_Status_PublishedAt");

        builder.HasIndex(r => new { r.CategoryId, r.Status })
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IDX_Recipe_Category_Status");

        builder.HasIndex(r => r.AuthorId)
            .HasDatabaseName("IDX_Recipe_AuthorId");

        // 12. Cấu hình quan hệ 1-N với các thực thể con do TV4 phụ trách
        builder.HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Images)
            .WithOne(img => img.Recipe)
            .HasForeignKey(img => img.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
