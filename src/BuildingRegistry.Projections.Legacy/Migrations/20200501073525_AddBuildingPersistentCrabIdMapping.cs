using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddBuildingPersistentCrabIdMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingPersistentIdCrabIdMappings",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    CrabTerrainObjectId = table.Column<int>(nullable: true),
                    CrabIdentifierTerrainObject = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingPersistentIdCrabIdMappings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingPersistentIdCrabIdMappings_CrabTerrainObjectId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingPersistentIdCrabIdMappings",
                column: "CrabTerrainObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingPersistentIdCrabIdMappings_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingPersistentIdCrabIdMappings",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingPersistentIdCrabIdMappings",
                schema: "BuildingRegistryLegacy");
        }
    }
}
