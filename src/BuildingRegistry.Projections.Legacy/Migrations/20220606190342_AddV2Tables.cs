using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddV2Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "BuildingDetailsV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    GeometryMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingDetailsV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    BuildingRetiredStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingsV2", x => x.BuildingPersistentLocalId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitDetailsV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitDetailsV2", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitSyndicationV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PointPosition = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
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
                name: "BuildingUnitAddressSyndicationV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_BuildingDetailsV2_IsRemoved_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                columns: new[] { "IsRemoved", "PersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetailsV2_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                column: "Status");

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
                name: "IX_BuildingUnitDetailsV2_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_IsRemoved_BuildingUnitPersistentLocalId_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                columns: new[] { "IsRemoved", "BuildingUnitPersistentLocalId", "BuildingPersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetailsV2_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "CI_BuildingUnitSyndicationV2_Position_BuildingUnitId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndicationV2",
                columns: new[] { "Position", "PersistentLocalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingDetailsV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressesV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddressSyndicationV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitDetailsV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "BuildingUnitSyndicationV2",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.AlterColumn<Guid>(
                name: "BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
