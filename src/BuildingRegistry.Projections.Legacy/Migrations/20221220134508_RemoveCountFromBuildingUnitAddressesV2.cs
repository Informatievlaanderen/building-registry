using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveCountFromBuildingUnitAddressesV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2");

            migrationBuilder.DropColumn(
                name: "Count",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressesV2");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2",
                column: "Position");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressSyndicationV2_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressesV2",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
