using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Menu.Infrastructure.Data;

#nullable disable

namespace Menu.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260428161000_OptimizePublicMenuReadIndexes")]
    public partial class OptimizePublicMenuReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Categories_MenuId_DisplayOrder",
                table: "Categories",
                columns: new[] { "MenuId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemImages_MenuItemId_DisplayOrder",
                table: "ItemImages",
                columns: new[] { "MenuItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemOptions_MenuItemId_Name",
                table: "ItemOptions",
                columns: new[] { "MenuItemId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_RestaurantId_IsActive_Name",
                table: "Menus",
                columns: new[] { "RestaurantId", "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId_IsAvailable_DisplayOrder",
                table: "MenuItems",
                columns: new[] { "CategoryId", "IsAvailable", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OptionValues_ItemOptionId_DisplayOrder",
                table: "OptionValues",
                columns: new[] { "ItemOptionId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_MenuId_DisplayOrder",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_ItemImages_MenuItemId_DisplayOrder",
                table: "ItemImages");

            migrationBuilder.DropIndex(
                name: "IX_ItemOptions_MenuItemId_Name",
                table: "ItemOptions");

            migrationBuilder.DropIndex(
                name: "IX_Menus_RestaurantId_IsActive_Name",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_CategoryId_IsAvailable_DisplayOrder",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_OptionValues_ItemOptionId_DisplayOrder",
                table: "OptionValues");
        }
    }
}
