using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class AddClusteredIndexAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
