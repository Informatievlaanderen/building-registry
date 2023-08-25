using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class DropDuplicateIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingsV2_PersistentLocalId",
                schema: "wfs",
                table: "BuildingsV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitsV2_BuildingUnitPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitsV2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_PersistentLocalId",
                schema: "wfs",
                table: "BuildingsV2",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_BuildingUnitPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "BuildingUnitPersistentLocalId");
        }
    }
}
