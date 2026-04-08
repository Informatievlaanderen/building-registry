using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Feed.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingUnitFeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "BuildingUnitFeedSequence",
                schema: "BuildingRegistryFeed");

            migrationBuilder.CreateTable(
                name: "BuildingGeometryForBuildingUnit",
                schema: "BuildingRegistryFeed",
                columns: table => new
                {
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    ExtendedWkbGeometry = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingGeometryForBuildingUnit", x => x.BuildingPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitDocuments",
                schema: "BuildingRegistryFeed",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitDocuments", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitFeed",
                schema: "BuildingRegistryFeed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudEvent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitFeed", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDocuments_BuildingPersistentLocalId",
                schema: "BuildingRegistryFeed",
                table: "BuildingUnitDocuments",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitFeed_BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryFeed",
                table: "BuildingUnitFeed",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitFeed_Page",
                schema: "BuildingRegistryFeed",
                table: "BuildingUnitFeed",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitFeed_Position",
                schema: "BuildingRegistryFeed",
                table: "BuildingUnitFeed",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingGeometryForBuildingUnit",
                schema: "BuildingRegistryFeed");

            migrationBuilder.DropTable(
                name: "BuildingUnitDocuments",
                schema: "BuildingRegistryFeed");

            migrationBuilder.DropTable(
                name: "BuildingUnitFeed",
                schema: "BuildingRegistryFeed");

            migrationBuilder.DropSequence(
                name: "BuildingUnitFeedSequence",
                schema: "BuildingRegistryFeed");
        }
    }
}
