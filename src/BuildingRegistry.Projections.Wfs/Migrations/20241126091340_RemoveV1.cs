using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class RemoveV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "BuildingUnits",
                schema: "wfs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buildings",
                schema: "wfs",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    GeometryMethod = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "wfs",
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

            migrationBuilder.CreateTable(
                name: "BuildingUnits",
                schema: "wfs",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Function = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsBuildingComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnits", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_GeometryMethod",
                schema: "wfs",
                table: "Buildings",
                column: "GeometryMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Id",
                schema: "wfs",
                table: "Buildings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_IsComplete_IsRemoved",
                schema: "wfs",
                table: "Buildings",
                columns: new[] { "IsComplete", "IsRemoved" });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_PersistentLocalId",
                schema: "wfs",
                table: "Buildings",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Status",
                schema: "wfs",
                table: "Buildings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_VersionAsString",
                schema: "wfs",
                table: "Buildings",
                column: "VersionAsString");

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
                name: "IX_BuildingUnits_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "wfs",
                table: "BuildingUnits",
                columns: new[] { "IsComplete", "IsRemoved", "IsBuildingComplete" });

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
        }
    }
}
