using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                newName: "BuildingPersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_BuildingUnitDetails_BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                newName: "IX_BuildingUnitDetails_BuildingPersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                newName: "BuildingPersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_BuildingUnit_Buildings_BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                newName: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_BuildingSyndication_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                newName: "IX_BuildingSyndication_PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                newName: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "[PersistentLocalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "[PersistentLocalId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                newName: "BuildingOsloId");

            migrationBuilder.RenameIndex(
                name: "IX_BuildingUnitDetails_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                newName: "IX_BuildingUnitDetails_BuildingOsloId");

            migrationBuilder.RenameColumn(
                name: "BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                newName: "BuildingOsloId");

            migrationBuilder.RenameIndex(
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                newName: "IX_BuildingUnit_Buildings_BuildingOsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_BuildingSyndication_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                newName: "IX_BuildingSyndication_OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                newName: "OsloId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "OsloId",
                unique: true,
                filter: "[OsloId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "OsloId",
                unique: true,
                filter: "[OsloId] IS NOT NULL");
        }
    }
}
