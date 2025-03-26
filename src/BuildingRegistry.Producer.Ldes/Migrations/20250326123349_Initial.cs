using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Producer.Ldes.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryProducerLdes");

            migrationBuilder.CreateTable(
                name: "Buildings",
                schema: "BuildingRegistryProducerLdes",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    GeometryMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnits",
                schema: "BuildingRegistryProducerLdes",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnits", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "BuildingRegistryProducerLdes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitAddresses",
                schema: "BuildingRegistryProducerLdes",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddresses", x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_BuildingUnitAddresses_BuildingUnits_BuildingUnitPersistentLocalId",
                        column: x => x.BuildingUnitPersistentLocalId,
                        principalSchema: "BuildingRegistryProducerLdes",
                        principalTable: "BuildingUnits",
                        principalColumn: "BuildingUnitPersistentLocalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_IsRemoved",
                schema: "BuildingRegistryProducerLdes",
                table: "Buildings",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddresses_AddressPersistentLocalId",
                schema: "BuildingRegistryProducerLdes",
                table: "BuildingUnitAddresses",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddresses_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryProducerLdes",
                table: "BuildingUnitAddresses",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_BuildingPersistentLocalId",
                schema: "BuildingRegistryProducerLdes",
                table: "BuildingUnits",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_IsRemoved",
                schema: "BuildingRegistryProducerLdes",
                table: "BuildingUnits",
                column: "IsRemoved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings",
                schema: "BuildingRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "BuildingUnitAddresses",
                schema: "BuildingRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "BuildingRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "BuildingUnits",
                schema: "BuildingRegistryProducerLdes");
        }
    }
}
