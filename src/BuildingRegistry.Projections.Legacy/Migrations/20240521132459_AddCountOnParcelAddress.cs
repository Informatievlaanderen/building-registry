using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class AddCountOnParcelAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingSyndicationWithCount",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventDataAsXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SyndicationItemCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingSyndicationWithCount", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitDetailsV2WithCount",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitDetailsV2WithCount", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitSyndicationV2WithCount",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PointPosition = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitSyndicationV2WithCount", x => new { x.Position, x.PersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitSyndicationV2WithCount_BuildingSyndicationWithCount_Position",
                        column: x => x.Position,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingSyndicationWithCount",
                        principalColumn: "Position",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitSyndicationWithCount",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    PointPosition = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitSyndicationWithCount", x => new { x.Position, x.BuildingUnitId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitSyndicationWithCount_BuildingSyndicationWithCount_Position",
                        column: x => x.Position,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingSyndicationWithCount",
                        principalColumn: "Position",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressesV2WithCount",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressesV2WithCount", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddressesV2WithCount_BuildingUnitDetailsV2WithCount_BuildingUnitPersistentLocalId",
                        column: x => x.BuildingUnitPersistentLocalId,
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitDetailsV2WithCount",
                        principalColumn: "BuildingUnitPersistentLocalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressSyndicationV2WithCount",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressSyndicationV2WithCount", x => new { x.Position, x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddressSyndicationV2WithCount_BuildingUnitSyndicationV2WithCount_Position_BuildingUnitPersistentLocalId",
                        columns: x => new { x.Position, x.BuildingUnitPersistentLocalId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndicationV2WithCount",
                        principalColumns: new[] { "Position", "PersistentLocalId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressSyndicationWithCount",
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
                    table.PrimaryKey("PK_BuildingUnitAddressSyndicationWithCount", x => new { x.Position, x.BuildingUnitId, x.AddressId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddressSyndicationWithCount_BuildingUnitSyndicationWithCount_Position_BuildingUnitId",
                        columns: x => new { x.Position, x.BuildingUnitId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndicationWithCount",
                        principalColumns: new[] { "Position", "BuildingUnitId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitReaddressSyndicationWithCount",
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
                    table.PrimaryKey("PK_BuildingUnitReaddressSyndicationWithCount", x => new { x.Position, x.BuildingUnitId, x.OldAddressId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitReaddressSyndicationWithCount_BuildingUnitSyndicationWithCount_Position_BuildingUnitId",
                        columns: x => new { x.Position, x.BuildingUnitId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndicationWithCount",
                        principalColumns: new[] { "Position", "BuildingUnitId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "CI_BuildingSyndicationWithCount_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndicationWithCount",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingSyndicationWithCount_BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndicationWithCount",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingSyndicationWithCount_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndicationWithCount",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressesV2WithCount_AddressPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressesV2WithCount",
                column: "AddressPersistentLocalId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressesV2WithCount_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressesV2WithCount",
                column: "BuildingUnitPersistentLocalId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2WithCount_AddressPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2WithCount",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2WithCount_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2WithCount",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressSyndicationV2WithCount_Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddressSyndicationV2WithCount",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2WithCount_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2WithCount",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2WithCount_Function",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2WithCount",
                column: "Function");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2WithCount_IsRemoved",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2WithCount",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2WithCount_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2WithCount",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "CI_BuildingUnitSyndicationV2WithCount_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndicationV2WithCount",
                columns: new[] { "Position", "PersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "CI_BuildingUnitSyndicationWithCount_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndicationWithCount",
                columns: new[] { "Position", "BuildingUnitId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddressesV2WithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndicationV2WithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndicationWithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitReaddressSyndicationWithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitDetailsV2WithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitSyndicationV2WithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitSyndicationWithCount",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingSyndicationWithCount",
                schema: "BuildingRegistryLegacy");
        }
    }
}
