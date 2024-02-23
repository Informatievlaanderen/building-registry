using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    public partial class AddEventTypeToVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "integration_building",
                table: "building_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "integration_building",
                table: "building_unit_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_type",
                schema: "integration_building",
                table: "building_versions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_type",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_building_versions_type",
                schema: "integration_building",
                table: "building_versions");

            migrationBuilder.DropIndex(
                name: "IX_building_unit_versions_type",
                schema: "integration_building",
                table: "building_unit_versions");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "integration_building",
                table: "building_versions");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "integration_building",
                table: "building_unit_versions");
        }
    }
}
