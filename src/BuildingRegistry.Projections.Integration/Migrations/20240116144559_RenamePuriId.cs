using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    public partial class RenamePuriId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "puri_id",
                schema: "integration_building",
                table: "building_unit_latest_items",
                newName: "puri");

            migrationBuilder.RenameColumn(
                name: "puri_id",
                schema: "integration_building",
                table: "building_latest_items",
                newName: "puri");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "puri",
                schema: "integration_building",
                table: "building_unit_latest_items",
                newName: "puri_id");

            migrationBuilder.RenameColumn(
                name: "puri",
                schema: "integration_building",
                table: "building_latest_items",
                newName: "puri_id");
        }
    }
}
