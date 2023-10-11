using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddV2Indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetailsV2_IsRemoved_BuildingUnitPersistentLocalId_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetailsV2_IsRemoved_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_IsRemoved",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetailsV2_IsRemoved",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetailsV2_IsRemoved_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                columns: new[] { "IsRemoved", "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetailsV2_IsRemoved",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetailsV2_IsRemoved",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetailsV2_IsRemoved_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_IsRemoved_BuildingUnitPersistentLocalId_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                columns: new[] { "IsRemoved", "BuildingUnitPersistentLocalId", "BuildingPersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetailsV2_IsRemoved_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                columns: new[] { "IsRemoved", "PersistentLocalId" });
        }
    }
}
