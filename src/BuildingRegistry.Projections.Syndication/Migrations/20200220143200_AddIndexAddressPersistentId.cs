using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class AddIndexAddressPersistentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PersistentLocalId",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressPersistentLocalIdSyndication_PersistentLocalId",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                column: "PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressPersistentLocalIdSyndication_PersistentLocalId",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication");

            migrationBuilder.AlterColumn<string>(
                name: "PersistentLocalId",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
