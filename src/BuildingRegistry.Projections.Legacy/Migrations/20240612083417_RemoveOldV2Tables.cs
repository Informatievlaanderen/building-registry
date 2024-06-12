using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    using Infrastructure;

    /// <inheritdoc />
    public partial class RemoveOldV2Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddressesV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndicationV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitReaddressSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitDetailsV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitSyndicationV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingSyndication",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.Sql(
                $"DELETE FROM [{Schema.Legacy}].[ProjectionStates] WHERE [Name] in ('BuildingRegistry.Projections.Legacy.BuildingDetail.BuildingDetailProjections', 'BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingSyndicationProjections', 'BuildingRegistry.Projections.Legacy.BuildingUnitDetail.BuildingUnitDetailProjections', 'BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2.BuildingUnitDetailV2Projections')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventDataAsXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<int>(type: "int", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    SyndicationItemCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitDetailsV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitDetailsV2", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    PointPosition = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                name: "BuildingUnitSyndicationV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PointPosition = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitSyndicationV2", x => new { x.Position, x.PersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitSyndicationV2_BuildingSyndication_Position",
                        column: x => x.Position,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingSyndication",
                        principalColumn: "Position",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressesV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressesV2", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddressesV2_BuildingUnitDetailsV2_BuildingUnitPersistentLocalId",
                        column: x => x.BuildingUnitPersistentLocalId,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitDetailsV2",
                        principalColumn: "BuildingUnitPersistentLocalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
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
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReaddressDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressSyndicationV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressSyndicationV2", x => new { x.Position, x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddressSyndicationV2_BuildingUnitSyndicationV2_Position_BuildingUnitPersistentLocalId",
                        columns: x => new { x.Position, x.BuildingUnitPersistentLocalId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndicationV2",
                        principalColumns: new[] { "Position", "PersistentLocalId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "CI_BuildingSyndication_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                column: "Position");

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
                name: "IX_BuildingUnitAddressesV2_AddressPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressesV2",
                column: "AddressPersistentLocalId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressesV2_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressesV2",
                column: "BuildingUnitPersistentLocalId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2_AddressPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_Function",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "Function");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_IsRemoved",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "CI_BuildingUnitSyndication_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication",
                columns: new[] { "Position", "BuildingUnitId" });

            migrationBuilder.CreateIndex(
                name: "CI_BuildingUnitSyndicationV2_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndicationV2",
                columns: new[] { "Position", "PersistentLocalId" });
        }
    }
}
