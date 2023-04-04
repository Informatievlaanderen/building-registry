using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddBuildingUnitAddressLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressLinks",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressLinks", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressLinks_AddressPersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnitAddressLinks",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressLinks_BuildingPersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnitAddressLinks",
                column: "BuildingPersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddressLinks",
                schema: "BuildingRegistryExtract");
        }
    }
}
