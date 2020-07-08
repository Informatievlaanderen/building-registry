using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class Initial_Building : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wfs");

            migrationBuilder.CreateTable(
                name: "Buildings",
                schema: "wfs",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    Id = table.Column<string>(nullable: true),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    GeometryMethod = table.Column<string>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: false),
                    VersionAsString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "wfs",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    DesiredState = table.Column<string>(nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
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
                name: "IX_Buildings_IsComplete_IsRemoved",
                schema: "wfs",
                table: "Buildings",
                columns: new[] { "IsComplete", "IsRemoved" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "wfs");
        }
    }
}
