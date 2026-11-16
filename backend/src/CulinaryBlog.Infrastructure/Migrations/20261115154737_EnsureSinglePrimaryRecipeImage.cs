using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureSinglePrimaryRecipeImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecipeImages_RecipeId",
                table: "RecipeImages");

            migrationBuilder.DropIndex(
                name: "IX_RecipeImages_RecipeId_IsPrimary",
                table: "RecipeImages");

            // Chuẩn hóa dữ liệu cũ trước khi bật ràng buộc; ưu tiên ảnh chính đã có.
            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT "Id", ROW_NUMBER() OVER (
                        PARTITION BY "RecipeId"
                        ORDER BY "IsPrimary" DESC, "OrderIndex", "CreatedAt", "Id"
                    ) AS position
                    FROM "RecipeImages" WHERE "IsDeleted" = false
                )
                UPDATE "RecipeImages" AS image SET "IsPrimary" = (ranked.position = 1)
                FROM ranked WHERE image."Id" = ranked."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImages_RecipeId_Primary_Unique",
                table: "RecipeImages",
                column: "RecipeId",
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecipeImages_RecipeId_Primary_Unique",
                table: "RecipeImages");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImages_RecipeId",
                table: "RecipeImages",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImages_RecipeId_IsPrimary",
                table: "RecipeImages",
                columns: new[] { "RecipeId", "IsPrimary" });
        }
    }
}
