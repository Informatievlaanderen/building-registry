using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingId_PositionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_building_versions_building_id_position",
                schema: "integration_building",
                table: "building_versions",
                columns: new[] { "building_id", "position" });

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_building_persistent_local_id_position",
                schema: "integration_building",
                table: "building_versions",
                columns: new[] { "building_persistent_local_id", "position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_building_versions_building_id_position",
                schema: "integration_building",
                table: "building_versions");

            migrationBuilder.DropIndex(
                name: "IX_building_versions_building_persistent_local_id_position",
                schema: "integration_building",
                table: "building_versions");
        }
    }
}
