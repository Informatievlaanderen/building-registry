using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class AddIndexBuilding_BuildingUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingId",
                schema: "wms",
                table: "BuildingUnits",
                column: "BuildingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_BuildingId",
                schema: "wms",
                table: "BuildingUnits");
        }
    }
}
