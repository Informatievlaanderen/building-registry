using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddBuildingUnitAddressIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddresses_AddressId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddresses",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddresses_AddressId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddresses");
        }
    }
}
