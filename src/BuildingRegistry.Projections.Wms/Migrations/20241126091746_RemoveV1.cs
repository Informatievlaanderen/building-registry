using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    /// <inheritdoc />
    public partial class RemoveV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnits",
                schema: "wms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "wms",
                table: "ProjectionStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "wms",
                table: "ProjectionStates",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "wms",
                table: "ProjectionStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "wms",
                table: "ProjectionStates",
                column: "Name")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateTable(
                name: "Buildings",
                schema: "wms",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true),
                    Id = table.Column<string>(type: "varchar(46)", maxLength: 46, nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "wms",
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
                schema: "wms",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Function = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: true),
                    Id = table.Column<string>(type: "varchar(53)", maxLength: 53, nullable: true),
                    IsBuildingComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "varchar(22)", maxLength: 22, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnits", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_IsComplete",
                schema: "wms",
                table: "Buildings",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Status",
                schema: "wms",
                table: "Buildings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "wms",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingId",
                schema: "wms",
                table: "BuildingUnits",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_IsComplete_IsBuildingComplete",
                schema: "wms",
                table: "BuildingUnits",
                columns: new[] { "IsComplete", "IsBuildingComplete" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_Status",
                schema: "wms",
                table: "BuildingUnits",
                column: "Status");
        }
    }
}
