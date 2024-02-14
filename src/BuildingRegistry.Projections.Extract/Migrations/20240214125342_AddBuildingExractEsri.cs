using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddBuildingExractEsri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingV2Esri",
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
                    table.PrimaryKey("PK_BuildingV2Esri", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2Esri_MaximumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2Esri",
                column: "MaximumX");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2Esri_MaximumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2Esri",
                column: "MaximumY");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2Esri_MinimumX",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2Esri",
                column: "MinimumX");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2Esri_MinimumY",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2Esri",
                column: "MinimumY");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingV2Esri_ShapeRecordContentLength",
                schema: "BuildingRegistryExtract",
                table: "BuildingV2Esri",
                column: "ShapeRecordContentLength");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingV2Esri",
                schema: "BuildingRegistryExtract");
        }
    }
}
