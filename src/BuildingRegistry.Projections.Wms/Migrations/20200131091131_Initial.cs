using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wms");

            migrationBuilder.CreateTable(
                name: "Buildings",
                schema: "wms",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    Id = table.Column<string>(type: "varchar(46)", maxLength: 46, nullable: true),
                    Geometry = table.Column<byte[]>(nullable: true),
                    GeometryMethod = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
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
                name: "BuildingUnit_BuildingPersistentLocalIds",
                schema: "wms",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingPersistentLocalId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingPersistentLocalIds", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnits",
                schema: "wms",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    Id = table.Column<string>(type: "varchar(53)", maxLength: 53, nullable: true),
                    BuildingUnitPersistentLocalId = table.Column<int>(nullable: true),
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(nullable: true),
                    Position = table.Column<byte[]>(nullable: true),
                    PositionMethod = table.Column<string>(type: "varchar(22)", maxLength: 22, nullable: true),
                    Function = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "wms",
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
                name: "IX_Buildings_Status",
                schema: "wms",
                table: "Buildings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingPersistentLocalIds_BuildingPersistentLocalId",
                schema: "wms",
                table: "BuildingUnit_BuildingPersistentLocalIds",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingId",
                schema: "wms",
                table: "BuildingUnits",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_Status",
                schema: "wms",
                table: "BuildingUnits",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingPersistentLocalIds",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnits",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "wms");
        }
    }
}
