using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class FixSyndicationTablesSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "BuildingParcelLatestItems",
                schema: "BuildingRegistryLegacy",
                newName: "BuildingParcelLatestItems",
                newSchema: "BuildingRegistrySyndication");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryLegacy");

            migrationBuilder.RenameTable(
                name: "BuildingParcelLatestItems",
                schema: "BuildingRegistrySyndication",
                newName: "BuildingParcelLatestItems",
                newSchema: "BuildingRegistryLegacy");
        }
    }
}
