using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class AddCaPaKeyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CaPaKey",
                schema: "BuildingRegistryLegacy",
                table: "BuildingParcelLatestItems",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingParcelLatestItems_CaPaKey",
                schema: "BuildingRegistryLegacy",
                table: "BuildingParcelLatestItems",
                column: "CaPaKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingParcelLatestItems_CaPaKey",
                schema: "BuildingRegistryLegacy",
                table: "BuildingParcelLatestItems");

            migrationBuilder.AlterColumn<string>(
                name: "CaPaKey",
                schema: "BuildingRegistryLegacy",
                table: "BuildingParcelLatestItems",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
