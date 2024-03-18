using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    public partial class AddBuildingUnitAddressVersionCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "count",
                schema: "integration_building",
                table: "building_unit_address_versions",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "count",
                schema: "integration_building",
                table: "building_unit_address_versions");
        }
    }
}
