using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingUnitAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnitAddresses",
                schema: "wfs",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddresses", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddresses_AddressPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitAddresses",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddresses_BuildingUnitPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitAddresses",
                column: "BuildingUnitPersistentLocalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddresses",
                schema: "wfs");
        }
    }
}
