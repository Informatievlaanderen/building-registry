using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddLastEventHashToDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastEventHash",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastEventHash",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEventHash",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2");

            migrationBuilder.DropColumn(
                name: "LastEventHash",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2");
        }
    }
}
