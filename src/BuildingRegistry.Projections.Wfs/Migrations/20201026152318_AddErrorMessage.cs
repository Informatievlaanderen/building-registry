using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class AddErrorMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "wfs",
                table: "ProjectionStates",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "wfs",
                table: "ProjectionStates");
        }
    }
}
