using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddBuildingQueryIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "ShapeRecordContentLength");

            migrationBuilder.CreateIndex(
                name: "IX_Building_IsComplete_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "Building",
                columns: new[] { "IsComplete", "ShapeRecordContentLength" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingV2_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2");

            migrationBuilder.DropIndex(
                name: "IX_Building_IsComplete_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "Building");
        }
    }
}
