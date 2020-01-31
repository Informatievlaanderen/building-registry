using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class AddStatusIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "wms",
                table: "BuildingUnits",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "wms",
                table: "Buildings",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_Status",
                schema: "wms",
                table: "BuildingUnits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Status",
                schema: "wms",
                table: "Buildings",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_Status",
                schema: "wms",
                table: "BuildingUnits");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_Status",
                schema: "wms",
                table: "Buildings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "wms",
                table: "BuildingUnits",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "wms",
                table: "Buildings",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
