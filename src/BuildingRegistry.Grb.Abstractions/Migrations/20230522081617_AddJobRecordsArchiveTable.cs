using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Grb.Abstractions.Migrations
{
    public partial class AddJobRecordsArchiveTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobRecordsArchive",
                schema: "BuildingRegistryGrb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Idn = table.Column<long>(type: "bigint", nullable: true),
                    IdnVersion = table.Column<int>(type: "int", nullable: true),
                    VersionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GrbObject = table.Column<int>(type: "int", nullable: true),
                    GrbObjectType = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: true),
                    GrId = table.Column<int>(type: "int", nullable: true),
                    Geometry = table.Column<Polygon>(type: "sys.geometry", nullable: true),
                    Overlap = table.Column<decimal>(type: "decimal(8,5)", precision: 8, scale: 5, nullable: true),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRecordsArchive", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobRecordsArchive_JobId",
                schema: "BuildingRegistryGrb",
                table: "JobRecordsArchive",
                column: "JobId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobRecordsArchive",
                schema: "BuildingRegistryGrb");
        }
    }
}
