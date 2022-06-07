using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class AddV2Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV2",
                schema: "wms",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "varchar(46)", maxLength: 46, nullable: true),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "wms",
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
                name: "BuildingUnitsV2",
                schema: "wms",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "varchar(53)", maxLength: 53, nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "varchar(22)", maxLength: 22, nullable: false),
                    Function = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitsV2", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_Status",
                schema: "wms",
                table: "BuildingsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_BuildingPersistentLocalId",
                schema: "wms",
                table: "BuildingUnitsV2",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV2_Status",
                schema: "wms",
                table: "BuildingUnitsV2",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingsV2",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnitsV2",
                schema: "wms");
        }
    }
}
