namespace CulinaryBlog.Infrastructure.Configurations;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình EF Core Fluent API cho thực thể RecipeStep (bảng RecipeSteps).
/// Tuân thủ đặc tả SRS v1.2.0 (mục 7.3) và quyết định kiến trúc E5, E8.
/// Phụ trách: Thành viên 4.
/// </summary>
public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        // 1. Tên bảng
        builder.ToTable("RecipeSteps");

        // 2. Khóa chính
        builder.HasKey(s => s.Id);

        // 3. Các thuộc tính
        builder.Property(s => s.RecipeId)
            .IsRequired();

        builder.Property(s => s.StepNumber)
            .IsRequired();

        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.DurationMinutes)
            .IsRequired(false);

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.ParentStepId)
            .IsRequired(false);

        // 4. Audit & Soft Delete (kế thừa BaseEntity)
        builder.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(s => s.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        // 5. Quan hệ với Recipe (Cascade Delete khi xóa Recipe vĩnh viễn)
        builder.HasOne(s => s.Recipe)
            .WithMany(r => r.Steps)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // 6. Quan hệ tự tham chiếu (Self-referencing) cho bước con tương lai (E8)
        builder.HasOne(s => s.ParentStep)
            .WithMany(p => p.SubSteps)
            .HasForeignKey(s => s.ParentStepId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // 7. Global Query Filter cho Soft Delete
        builder.HasQueryFilter(s => !s.IsDeleted);

        // 8. Composite Unique Index trên (RecipeId, StepNumber) khi IsDeleted = false
        builder.HasIndex(s => new { s.RecipeId, s.StepNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_RecipeSteps_RecipeId_StepNumber");

        // 9. Index hỗ trợ truy vấn các bước theo Recipe và ParentStepId
        builder.HasIndex(s => s.RecipeId)
            .HasDatabaseName("IX_RecipeSteps_RecipeId");

        builder.HasIndex(s => s.ParentStepId)
            .HasDatabaseName("IX_RecipeSteps_ParentStepId");
    }
}
