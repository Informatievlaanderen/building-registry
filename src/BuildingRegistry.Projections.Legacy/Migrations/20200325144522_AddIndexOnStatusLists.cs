using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddIndexOnStatusLists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");

            migrationBuilder.DropIndex(
                name: "IX_BuildingDetails_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
