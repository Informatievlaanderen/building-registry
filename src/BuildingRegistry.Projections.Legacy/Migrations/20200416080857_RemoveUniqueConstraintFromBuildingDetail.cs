using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveUniqueConstraintFromBuildingDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "PersistentLocalId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
