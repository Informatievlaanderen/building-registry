using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddExtractV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    BuildingRetiredStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingsV2", x => x.BuildingPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitV2",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitV2", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingV2",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitV2_BuildingPersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnitV2",
                column: "BuildingPersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "BuildingRegistryExtract");

            migrationBuilder.DropTable(
                name: "BuildingUnitV2",
                schema: "BuildingRegistryExtract");

            migrationBuilder.DropTable(
                name: "BuildingV2",
                schema: "BuildingRegistryExtract");
        }
    }
}
