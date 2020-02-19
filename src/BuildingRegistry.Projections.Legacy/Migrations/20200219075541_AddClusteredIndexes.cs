using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddClusteredIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "PersistentLocalId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
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
    }
}
