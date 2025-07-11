using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitChain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetDaysToHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetDays",
                table: "Habits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Badges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorTheme",
                table: "Badges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Badges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Badges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSecret",
                table: "Badges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "Badges",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetDays",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "ColorTheme",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "IsSecret",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "Badges");
        }
    }
}
