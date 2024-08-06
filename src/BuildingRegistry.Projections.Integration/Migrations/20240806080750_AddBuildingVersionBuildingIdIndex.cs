using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingVersionBuildingIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_building_versions_building_id",
                schema: "integration_building",
                table: "building_versions",
                column: "building_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_building_versions_building_id",
                schema: "integration_building",
                table: "building_versions");
        }
    }
}
