using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Grb.Abstractions.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryGrb");

            migrationBuilder.CreateTable(
                name: "JobRecords",
                schema: "BuildingRegistryGrb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Idn = table.Column<long>(type: "bigint", nullable: false),
                    IdnVersion = table.Column<int>(type: "int", nullable: false),
                    VersionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GrbObject = table.Column<int>(type: "int", nullable: false),
                    GrbObjectType = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    GrId = table.Column<int>(type: "int", nullable: false),
                    Geometry = table.Column<Polygon>(type: "sys.geometry", nullable: false),
                    Overlap = table.Column<decimal>(type: "decimal(8,5)", precision: 8, scale: 5, nullable: true),
                    TicketUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRecords", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "JobResults",
                schema: "BuildingRegistryGrb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrbIdn = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsBuildingCreated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobResults", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "BuildingRegistryGrb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastChanged = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TicketUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobRecords_JobId",
                schema: "BuildingRegistryGrb",
                table: "JobRecords",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                schema: "BuildingRegistryGrb",
                table: "Jobs",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobRecords",
                schema: "BuildingRegistryGrb");

            migrationBuilder.DropTable(
                name: "JobResults",
                schema: "BuildingRegistryGrb");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "BuildingRegistryGrb");
        }
    }
}
