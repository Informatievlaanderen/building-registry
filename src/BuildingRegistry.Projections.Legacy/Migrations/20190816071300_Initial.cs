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
                    PersistentLocalId = table.Column<int>(nullable: true),
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
                    PersistentLocalId = table.Column<int>(nullable: true),
                    ChangeType = table.Column<string>(nullable: true),
                    Geometry = table.Column<byte[]>(nullable: true),
                    GeometryMethod = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(nullable: false),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    EventDataAsXml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryLegacy",
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
                name: "BuildingUnitDetails",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(nullable: true),
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
                    PersistentLocalId = table.Column<int>(nullable: true),
                    PointPosition = table.Column<byte[]>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    Function = table.Column<string>(nullable: true),
                    PositionMethod = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "BuildingUnitReaddressSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    OldAddressId = table.Column<Guid>(nullable: false),
                    NewAddressId = table.Column<Guid>(nullable: false),
                    ReaddressDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitReaddressSyndication", x => new { x.Position, x.BuildingUnitId, x.OldAddressId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitReaddressSyndication_BuildingUnitSyndication_Position_BuildingUnitId",
                        columns: x => new { x.Position, x.BuildingUnitId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndication",
                        principalColumns: new[] { "Position", "BuildingUnitId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "[PersistentLocalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingSyndication_BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingSyndication_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "[PersistentLocalId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingDetails",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddresses",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitReaddressSyndication",
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
