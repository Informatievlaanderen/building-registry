using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    using Infrastructure;

    /// <summary>
    /// The Lambert 2008 (EPSG 3812) counterparts of [wfs].[BuildingsV3] and [wfs].[BuildingUnitsV2], with
    /// their own spatial indexes and views. Both versions run side by side until the geoserver consumers
    /// have moved over. See ADR 0005.
    /// </summary>
    public partial class AddWfsV4_Lambert2008 : Migration
    {
        private const string BuildingViewName = "GebouwViewV4";
        private const string BuildingUnitViewName = "GebouweenheidViewV3";

        /// <summary>
        /// The Lambert 72 box the V3 and V2 indexes use, (22279.17, 153050.23, 258873.3, 244022.31), with all
        /// four corners transformed to Lambert 2008 and the resulting envelope padded out to the next 100 m.
        /// A conformal projection does not map a rectangle to a rectangle, so the corners cannot be
        /// transformed pairwise. See ADR 0005.
        /// </summary>
        private const string BoundingBox = "522200, 653000, 758900, 744100";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV4",
                schema: "wfs",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    GeometryMethod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV4", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitsV3",
                schema: "wfs",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<Geometry>(type: "sys.geometry", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Function = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitsV3", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_GeometryMethod",
                schema: "wfs",
                table: "BuildingsV4",
                column: "GeometryMethod");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_Id",
                schema: "wfs",
                table: "BuildingsV4",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_IsRemoved",
                schema: "wfs",
                table: "BuildingsV4",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_PersistentLocalId",
                schema: "wfs",
                table: "BuildingsV4",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_Status",
                schema: "wfs",
                table: "BuildingsV4",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_VersionAsString",
                schema: "wfs",
                table: "BuildingsV4",
                column: "VersionAsString");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_BuildingPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_BuildingUnitPersistentLocalId",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "BuildingUnitPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_Function",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "Function");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_Id",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_IsRemoved",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_PositionMethod",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "PositionMethod");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_Status",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_VersionAsString",
                schema: "wfs",
                table: "BuildingUnitsV3",
                column: "VersionAsString");

            migrationBuilder.Sql($@"
	            CREATE SPATIAL INDEX [SPATIAL_BuildingsV4_Geometry] ON [{Schema.Wfs}].[BuildingsV4] ([Geometry])
	            USING  GEOMETRY_GRID
	            WITH (
		            BOUNDING_BOX =({BoundingBox}),
		            GRIDS =(
			            LEVEL_1 = MEDIUM,
			            LEVEL_2 = MEDIUM,
			            LEVEL_3 = MEDIUM,
			            LEVEL_4 = MEDIUM),
	            CELLS_PER_OBJECT = 5)
	            GO");

            migrationBuilder.Sql($@"
                CREATE SPATIAL INDEX [SPATIAL_BuildingUnitsV3_Position] ON [{Schema.Wfs}].[BuildingUnitsV3] ([Position])
                USING  GEOMETRY_GRID
                WITH (
                    BOUNDING_BOX =({BoundingBox}),
                    GRIDS =(
                            LEVEL_1 = MEDIUM,
                            LEVEL_2 = MEDIUM,
                            LEVEL_3 = MEDIUM,
                            LEVEL_4 = MEDIUM),
                    CELLS_PER_OBJECT = 5
                )
                GO");

            // Identical to [wfs].[GebouwView], apart from the source table.
            migrationBuilder.Sql($@"
                CREATE VIEW [{Schema.Wfs}].[{BuildingViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    [PersistentLocalId] AS [MyId],
                    [Id],
                    [PersistentLocalId] AS [ObjectId],
                    [VersionAsString] AS [VersieId],
                    [Geometry] AS [Geometrie],
                    [GeometryMethod] AS [GeometrieMethode],
                    [Status] AS [GebouwStatus]
                FROM [{Schema.Wfs}].[{BuildingV4.BuildingConfiguration.TableName}]
                WHERE [IsRemoved] = 0 and Geometry is not null");

            // Identical to [wfs].[GebouweenheidView], apart from the source table.
            migrationBuilder.Sql($@"
                CREATE VIEW [{Schema.Wfs}].[{BuildingUnitViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    [BuildingUnitPersistentLocalId] AS [MyId],
                    [Id],
                    [BuildingUnitPersistentLocalId] AS [ObjectId],
                    [VersionAsString] AS [VersieId],
                    [Position] AS [Geometrie],
                    [PositionMethod] AS [PositieGeometrieMethode],
                    [Status] AS [GebouweenheidStatus],
                    [Function] AS [Functie],
                    [HasDeviation] As [AfwijkingVastgesteld],
                    [BuildingPersistentLocalId] AS [GebouwObjectId]
                FROM [{Schema.Wfs}].[{BuildingUnitV3.BuildingUnitConfiguration.TableName}]
                WHERE [IsRemoved] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The views are SCHEMABINDING, so they have to go before the tables they bind to.
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingViewName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingUnitViewName}]");

            migrationBuilder.DropTable(
                name: "BuildingsV4",
                schema: "wfs");

            migrationBuilder.DropTable(
                name: "BuildingUnitsV3",
                schema: "wfs");
        }
    }
}
