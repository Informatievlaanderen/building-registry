using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddBuildingIndexOnBuildingUnitDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "BuildingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");
        }
    }
}
