using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class ColumnIndexSyndicationBuildingUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "CI_BuildingUnitSyndication_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication",
                columns: new[] { "Position", "BuildingUnitId" })
                .Annotation("SqlServer:ColumnStoreIndex", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "CI_BuildingUnitSyndication_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication");
        }
    }
}
