using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WriteRecipeNutritionSourceExplicitly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Nutrition_Source",
                table: "Recipes",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true,
                oldDefaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Nutrition_Source",
                table: "Recipes",
                type: "smallint",
                nullable: true,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }
    }
}
