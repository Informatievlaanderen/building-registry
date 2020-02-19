using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddClusteredIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnit_BuildingId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnit_BuildingId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "BuildingId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "PersistentLocalId");
        }
    }
}
