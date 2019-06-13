using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "BuildingDetails",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    GeometryMethod = table.Column<int>(nullable: true),
                    Geometry = table.Column<byte[]>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    Version = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingDetails", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    ChangeType = table.Column<string>(nullable: true),
                    GeometryWkbHex = table.Column<string>(nullable: true),
                    GeometryMethod = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(nullable: false),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Plan = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingOsloIds",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingOsloId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingOsloIds", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitDetails",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    BuildingOsloId = table.Column<int>(nullable: true),
                    Position = table.Column<byte[]>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    IsBuildingComplete = table.Column<bool>(nullable: false),
                    Function = table.Column<string>(nullable: true),
                    PositionMethod = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitDetails", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    PositionWkbHex = table.Column<string>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    Function = table.Column<string>(nullable: true),
                    PositionMethod = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitSyndication", x => new { x.Position, x.BuildingUnitId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitSyndication_BuildingSyndication_Position",
                        column: x => x.Position,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingSyndication",
                        principalColumn: "Position",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddresses",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddresses", x => new { x.BuildingUnitId, x.AddressId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddresses_BuildingUnitDetails_BuildingUnitId",
                        column: x => x.BuildingUnitId,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitDetails",
                        principalColumn: "BuildingUnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressSyndication", x => new { x.Position, x.BuildingUnitId, x.AddressId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddressSyndication_BuildingUnitSyndication_Position_BuildingUnitId",
                        columns: x => new { x.Position, x.BuildingUnitId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndication",
                        principalColumns: new[] { "Position", "BuildingUnitId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "OsloId",
                unique: true,
                filter: "[OsloId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingSyndication_BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingSyndication_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                column: "OsloId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingOsloIds_BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_BuildingOsloIds",
                column: "BuildingOsloId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "BuildingOsloId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_OsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "OsloId",
                unique: true,
                filter: "[OsloId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingDetails",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingOsloIds",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddresses",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitDetails",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingSyndication",
                schema: "BuildingRegistryLegacy");
        }
    }
}
