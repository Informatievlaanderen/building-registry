using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddCrabMappingGrbIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CrabIdentifierTerrainObject",
                schema: "BuildingRegistryLegacy",
                table: "BuildingPersistentIdCrabIdMappings",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingPersistentIdCrabIdMappings_CrabIdentifierTerrainObject",
                schema: "BuildingRegistryLegacy",
                table: "BuildingPersistentIdCrabIdMappings",
                column: "CrabIdentifierTerrainObject");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingPersistentIdCrabIdMappings_CrabIdentifierTerrainObject",
                schema: "BuildingRegistryLegacy",
                table: "BuildingPersistentIdCrabIdMappings");

            migrationBuilder.AlterColumn<string>(
                name: "CrabIdentifierTerrainObject",
                schema: "BuildingRegistryLegacy",
                table: "BuildingPersistentIdCrabIdMappings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
