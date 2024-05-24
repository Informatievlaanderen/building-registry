using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class DeleteV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "BuildingUnitDetails",
                schema: "BuildingRegistryLegacy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingDetails",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<int>(type: "int", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingDetails", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryLegacy",
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
                name: "BuildingUnitDetails",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBuildingComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitDetails", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddresses",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_IsComplete_IsRemoved_PersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                columns: new[] { "IsComplete", "IsRemoved", "PersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_PersistentLocalId_1",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDetails_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddresses_AddressId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitAddresses",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_BuildingId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_IsComplete_IsRemoved_PersistentLocalId_IsBuildingComplete_BuildingPersistentLocalId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                columns: new[] { "IsComplete", "IsRemoved", "PersistentLocalId", "IsBuildingComplete", "BuildingPersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId_1",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_Status",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "Status");
        }
    }
}
