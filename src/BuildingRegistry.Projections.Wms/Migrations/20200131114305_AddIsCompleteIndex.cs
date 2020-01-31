using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class AddIsCompleteIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_IsComplete_IsBuildingComplete",
                schema: "wms",
                table: "BuildingUnits",
                columns: new[] { "IsComplete", "IsBuildingComplete" });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_IsComplete",
                schema: "wms",
                table: "Buildings",
                column: "IsComplete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_IsComplete_IsBuildingComplete",
                schema: "wms",
                table: "BuildingUnits");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_IsComplete",
                schema: "wms",
                table: "Buildings");
        }
    }
}
