using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    /// <inheritdoc />
    public partial class AddCountOnParcelAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressLinksWithCount",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressLinksWithCount", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressLinksWithCount_AddressPersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnitAddressLinksWithCount",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressLinksWithCount_BuildingPersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnitAddressLinksWithCount",
                column: "BuildingPersistentLocalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddressLinksWithCount",
                schema: "BuildingRegistryExtract");
        }
    }
}
