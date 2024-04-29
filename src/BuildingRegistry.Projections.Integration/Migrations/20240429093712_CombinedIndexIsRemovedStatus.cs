using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class CombinedIndexIsRemovedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_building_unit_latest_items_is_removed_status",
                schema: "integration_building",
                table: "building_unit_latest_items",
                columns: new[] { "is_removed", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_building_latest_items_is_removed_status",
                schema: "integration_building",
                table: "building_latest_items",
                columns: new[] { "is_removed", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_building_unit_latest_items_is_removed_status",
                schema: "integration_building",
                table: "building_unit_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_building_latest_items_is_removed_status",
                schema: "integration_building",
                table: "building_latest_items");
        }
    }
}
