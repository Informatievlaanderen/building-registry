using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class RemoveParcelComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComplete",
                schema: "BuildingRegistrySyndication",
                table: "BuildingParcelLatestItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                schema: "BuildingRegistrySyndication",
                table: "BuildingParcelLatestItems",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
