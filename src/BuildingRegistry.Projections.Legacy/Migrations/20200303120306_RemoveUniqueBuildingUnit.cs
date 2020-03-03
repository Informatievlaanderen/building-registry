using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveUniqueBuildingUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
