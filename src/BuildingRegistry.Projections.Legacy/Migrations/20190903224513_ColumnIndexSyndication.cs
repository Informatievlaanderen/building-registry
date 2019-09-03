using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class ColumnIndexSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "CI_BuildingSyndication_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                column: "Position")
                .Annotation("SqlServer:ColumnStoreIndex", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "CI_BuildingSyndication_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication");
        }
    }
}
