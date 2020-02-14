using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddIndexesOnLists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_IsComplete_IsRemoved_PersistentLocalId_IsBuildingComplete_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                columns: new[] { "IsComplete", "IsRemoved", "PersistentLocalId", "IsBuildingComplete", "BuildingPersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_IsComplete_IsRemoved_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                columns: new[] { "IsComplete", "IsRemoved", "PersistentLocalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_IsComplete_IsRemoved_PersistentLocalId_IsBuildingComplete_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_IsComplete_IsRemoved_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");
        }
    }
}
