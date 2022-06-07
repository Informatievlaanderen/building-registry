using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class AddV2Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV2",
                schema: "wfs",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    GeometryMethod = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "wfs",
                columns: table => new
                {
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    BuildingRetiredStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingsV2", x => x.BuildingPersistentLocalId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitsV2",
                schema: "wfs",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Function = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitsV2", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_GeometryMethod",
                schema: "wfs",
                table: "BuildingsV2",
                column: "GeometryMethod");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_Id",
                schema: "wfs",
                table: "BuildingsV2",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_IsRemoved",
                schema: "wfs",
                table: "BuildingsV2",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_PersistentLocalId",
                schema: "wfs",
                table: "BuildingsV2",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_Status",
                schema: "wfs",
                table: "BuildingsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_VersionAsString",
                schema: "wfs",
                table: "BuildingsV2",
                column: "VersionAsString");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingsV2_BuildingPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnit_BuildingsV2",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_BuildingPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_BuildingUnitPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_Function",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "Function");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_Id",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_IsRemoved",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_PositionMethod",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "PositionMethod");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_Status",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_VersionAsString",
                schema: "wfs",
                table: "BuildingUnitsV2",
                column: "VersionAsString");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingsV2",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "BuildingUnitsV2",
                schema: "wfs");
        }
    }
}
