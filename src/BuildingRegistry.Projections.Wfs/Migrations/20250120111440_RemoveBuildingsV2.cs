using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBuildingsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingsV2",
                schema: "wfs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV2",
                schema: "wfs",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    GeometryMethod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV2", x => x.PersistentLocalId)
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
        }
    }
}
