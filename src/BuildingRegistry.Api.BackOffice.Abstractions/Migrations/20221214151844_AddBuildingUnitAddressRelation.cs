using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Api.BackOffice.Abstractions.Migrations
{
    public partial class AddBuildingUnitAddressRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressRelation",
                schema: "BuildingRegistryBackOffice",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressRelation", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressRelation_AddressPersistentLocalId",
                schema: "BuildingRegistryBackOffice",
                table: "BuildingUnitAddressRelation",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressRelation_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryBackOffice",
                table: "BuildingUnitAddressRelation",
                column: "BuildingUnitPersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddressRelation",
                schema: "BuildingRegistryBackOffice");
        }
    }
}
