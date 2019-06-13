using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddXmlEventDataSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventDataAsXml",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDataAsXml",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication");
        }
    }
}
