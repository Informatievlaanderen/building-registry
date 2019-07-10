using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "BuildingRegistryExtract",
                table: "Building",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_Building_OsloId",
                schema: "BuildingRegistryExtract",
                table: "Building",
                newName: "IX_Building_PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "Building",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_Building_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "Building",
                newName: "IX_Building_OsloId");
        }
    }
}
