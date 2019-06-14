using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class RenamePlanToReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication");

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                nullable: true);
        }
    }
}
