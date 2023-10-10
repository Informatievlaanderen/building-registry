using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddStringVersionsOfEnums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Function",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_Function",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "Function");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetailsV2_Function",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2");

            migrationBuilder.AlterColumn<string>(
                name: "Function",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
