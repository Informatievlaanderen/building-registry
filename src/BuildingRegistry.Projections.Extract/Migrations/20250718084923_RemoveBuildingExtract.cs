using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBuildingExtract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingV2",
                schema: "BuildingRegistryExtract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingV2",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MaximumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MaximumX");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MaximumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MaximumY");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MinimumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MinimumX");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_MinimumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "MinimumY");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2",
                column: "ShapeRecordContentLength");
        }
    }
}
