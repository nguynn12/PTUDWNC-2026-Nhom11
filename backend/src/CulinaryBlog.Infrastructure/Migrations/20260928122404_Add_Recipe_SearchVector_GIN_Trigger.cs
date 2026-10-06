using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Recipe_SearchVector_GIN_Trigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Recipes",
                type: "tsvector",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_SearchVector",
                table: "Recipes",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            // 1. Tạo Function cập nhật SearchVector tự động (Title: Weight A, Description: Weight B, unaccent, simple)
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION recipes_search_vector_update() RETURNS trigger AS $$
                BEGIN
                    NEW.""SearchVector"" :=
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Title"", ''))), 'A') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Description"", ''))), 'B');
                    RETURN NEW;
                END
                $$ LANGUAGE plpgsql;
            ");

            // 2. Tạo Trigger thực thi hàm recipes_search_vector_update khi INSERT hoặc UPDATE Title, Description
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_recipes_search_vector_update
                BEFORE INSERT OR UPDATE OF ""Title"", ""Description"" ON ""Recipes""
                FOR EACH ROW
                EXECUTE FUNCTION recipes_search_vector_update();
            ");

            // 3. Khởi tạo giá trị SearchVector cho các bản ghi Recipes hiện có (nếu có)
            migrationBuilder.Sql(@"
                UPDATE ""Recipes""
                SET ""SearchVector"" =
                    setweight(to_tsvector('simple', unaccent(coalesce(""Title"", ''))), 'A') ||
                    setweight(to_tsvector('simple', unaccent(coalesce(""Description"", ''))), 'B');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipes_search_vector_update ON ""Recipes"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS recipes_search_vector_update();");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_SearchVector",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Recipes");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
