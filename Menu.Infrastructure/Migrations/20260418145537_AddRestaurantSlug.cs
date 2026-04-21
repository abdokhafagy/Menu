using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Restaurants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Slug",
                table: "Restaurants",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Slug",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Restaurants");
        }
    }
}
