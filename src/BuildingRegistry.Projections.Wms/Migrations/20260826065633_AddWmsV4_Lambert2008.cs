using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    using System.Linq;

    /// <summary>
    /// The Lambert 2008 (EPSG 3812) counterparts of [wms].[BuildingsV3] and [wms].[BuildingUnitsV2], with
    /// their own computed columns, spatial indexes and views. Both versions run side by side until the
    /// geoserver consumers have moved over. See ADR 0005.
    /// </summary>
    public partial class AddWmsV4_Lambert2008 : Migration
    {
        private readonly AddViews.StatusViews _buildingViews = new AddViews.StatusViews(
            "GebouwViewV4",
            new AddViews.StatusView { Name = "GebouwGeplandV4", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerdV4", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerdV4", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerdV4", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouwV4", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly AddViews.StatusViews _buildingUnitViews = new AddViews.StatusViews(
            "GebouweenheidViewV3",
            new AddViews.StatusView { Name = "GebouweenheidGehistoreerdV3", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouweenheidGeplandV3", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouweenheidGerealiseerdV3", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouweenheidNietGerealiseerdV3", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" }
        );

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
                schema: "wms",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "varchar(46)", maxLength: 46, nullable: true),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV4", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnitsV3",
                schema: "wms",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "varchar(53)", maxLength: 53, nullable: true),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<string>(type: "varchar(22)", maxLength: 22, nullable: false),
                    Function = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HasDeviation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitsV3", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV4_Status",
                schema: "wms",
                table: "BuildingsV4",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_BuildingPersistentLocalId",
                schema: "wms",
                table: "BuildingUnitsV3",
                column: "BuildingPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitsV3_Status",
                schema: "wms",
                table: "BuildingUnitsV3",
                column: "Status");

            CreateBuildingViews(migrationBuilder);
            CreateBuildingUnitViews(migrationBuilder);
        }

        private void CreateBuildingViews(MigrationBuilder migrationBuilder)
        {
            // 3812, not the 31370 the V3 table uses: the projection writes plain WKB, which carries no SRID,
            // so this column is what decides the reference system the geoserver serves. See ADR 0005.
            migrationBuilder.Sql($@"
	            ALTER TABLE [wms].[{BuildingV4.BuildingConfiguration.TableName}]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Geometry], 3812)) PERSISTED
	            GO");

            migrationBuilder.Sql($@"
	            CREATE SPATIAL INDEX [SPATIAL_GebouwV4_Geometrie] ON [wms].[{BuildingV4.BuildingConfiguration.TableName}] ([CalculatedGeometry])
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

            // Identical to [wms].[GebouwView], apart from the source table.
            migrationBuilder.Sql($@"
                CREATE VIEW [wms].[{_buildingViews.SourceViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    [PersistentLocalId] AS [ObjectId],
                    [Id],
                    [VersionAsString] AS [VersieId],
                    [CalculatedGeometry] AS [Geometry],
                    [GeometryMethod] AS [GeometrieMethode],
                    [Status],
                    [Version] AS RawVersion
                FROM [wms].[{BuildingV4.BuildingConfiguration.TableName}]
                WHERE ([CalculatedGeometry] IS NOT NULL)
                GO");

            foreach (var view in _buildingViews.Views)
                migrationBuilder.Sql($@"
                    CREATE VIEW [wms].[{view.Name}]
                    WITH SCHEMABINDING
                    AS
                    SELECT
                        [ObjectId],
                        [Id],
                        [VersieId],
                        [Geometry],
                        [GeometrieMethode],
                        '{view.DisplayedStatus}' as [GebouwStatus]
                    FROM [wms].[{_buildingViews.SourceViewName}]
                    WHERE Status = '{view.Criteria}'
                    GO");
        }

        private void CreateBuildingUnitViews(MigrationBuilder migrationBuilder)
        {
            // 3812, not the 31370 the V2 table uses. See ADR 0005.
            migrationBuilder.Sql($@"
	            ALTER TABLE [wms].[{BuildingUnitV3.BuildingUnitConfiguration.TableName}]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Position], 3812)) PERSISTED
	            GO");

            migrationBuilder.Sql($@"
                CREATE SPATIAL INDEX [SPATIAL_GebouweenheidV3_Geometry] ON [wms].[{BuildingUnitV3.BuildingUnitConfiguration.TableName}] ([CalculatedGeometry])
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

            // Identical to [wms].[GebouweenheidView], apart from the source table.
            migrationBuilder.Sql($@"
                CREATE VIEW [wms].[{_buildingUnitViews.SourceViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    [Id],
                    [BuildingUnitPersistentLocalId] AS [ObjectId],
                    [VersionAsString] AS [VersieId],
                    [PositionMethod] AS [PositieGeometrieMethode],
                    [Status] AS [GebouweenheidStatus],
                    [Function] AS [Functie],
                    [BuildingPersistentLocalId] AS [GebouwObjectId],
                    [CalculatedGeometry] AS [Geometry],
                    [HasDeviation] As [AfwijkingVastgesteld],
                    [Version] AS RawVersion
                FROM [wms].[{BuildingUnitV3.BuildingUnitConfiguration.TableName}]
                GO");

            foreach (var view in _buildingUnitViews.Views)
                migrationBuilder.Sql($@"
                    CREATE VIEW [wms].[{view.Name}]
                    WITH SCHEMABINDING
                    AS
                    SELECT
                        [Id],
                        [ObjectId],
                        [VersieId],
                        [PositieGeometrieMethode],
                        '{view.DisplayedStatus}' as [GebouweenheidStatus],
                        [Functie],
                        [AfwijkingVastgesteld],
                        [GebouwObjectId],
                        [Geometry]
                    FROM [wms].[{_buildingUnitViews.SourceViewName}]
                    WHERE GebouweenheidStatus = '{view.Criteria}'
                    GO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The status views are SCHEMABINDING over their source view, so the source goes last, and all of
            // them go before the tables they ultimately bind to.
            Drop(_buildingUnitViews);
            Drop(_buildingViews);

            migrationBuilder.DropTable(
                name: "BuildingsV4",
                schema: "wms");

            migrationBuilder.DropTable(
                name: "BuildingUnitsV3",
                schema: "wms");

            void Drop(AddViews.StatusViews statusViews)
            {
                var query = statusViews.Views
                    .Select(view => view.Name)
                    .Concat(new[] { statusViews.SourceViewName })
                    .Aggregate("", (current, view) => current + $@"
                        DROP VIEW [wms].[{view}]
                        GO");

                migrationBuilder.Sql(query);
            }
        }
    }
}
