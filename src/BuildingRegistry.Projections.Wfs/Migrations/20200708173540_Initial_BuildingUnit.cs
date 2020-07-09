using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class Initial_BuildingUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "wfs",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: true),
                    IsRemoved = table.Column<bool>(nullable: false),
                    BuildingRetiredStatus = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_Buildings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnits",
                schema: "wfs",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    Id = table.Column<string>(nullable: true),
                    BuildingUnitPersistentLocalId = table.Column<int>(nullable: true),
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(nullable: true),
                    Position = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    PositionMethod = table.Column<string>(nullable: true),
                    Function = table.Column<string>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    IsBuildingComplete = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: false),
                    VersionAsString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnits", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingId",
                schema: "wfs",
                table: "BuildingUnits",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnits",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingUnitPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnits",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_Function",
                schema: "wfs",
                table: "BuildingUnits",
                column: "Function");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_Id",
                schema: "wfs",
                table: "BuildingUnits",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_PositionMethod",
                schema: "wfs",
                table: "BuildingUnits",
                column: "PositionMethod");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_Status",
                schema: "wfs",
                table: "BuildingUnits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_VersionAsString",
                schema: "wfs",
                table: "BuildingUnits",
                column: "VersionAsString");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "wfs",
                table: "BuildingUnits",
                columns: new[] { "IsComplete", "IsRemoved", "IsBuildingComplete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "BuildingUnits",
                schema: "wfs");
        }
    }
}
