using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddBoundingBoxIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MaximumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MaximumX");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MaximumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MaximumY");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MinimumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MinimumX");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MinimumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MinimumY");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingV2_MaximumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingV2_MaximumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingV2_MinimumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2");

            migrationBuilder.DropIndex(
                name: "IX_BuildingV2_MinimumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2");
        }
    }
}
