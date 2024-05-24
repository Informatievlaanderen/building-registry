using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    /// <inheritdoc />
    public partial class DeleteV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Building",
                schema: "BuildingRegistryExtract");

            migrationBuilder.DropTable(
                name: "BuildingUnit",
                schema: "BuildingRegistryExtract");

            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryExtract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Building",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Building", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IsBuildingComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    BuildingRetiredStatus = table.Column<int>(type: "int", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_Buildings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Building_IsComplete_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "Building",
                columns: new[] { "IsComplete", "ShapeRecordContentLength" });

            migrationBuilder.CreateIndex(
                name: "IX_Building_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "Building",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_IsComplete_IsBuildingComplete",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                columns: new[] { "IsComplete", "IsBuildingComplete" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");
        }
    }
}
