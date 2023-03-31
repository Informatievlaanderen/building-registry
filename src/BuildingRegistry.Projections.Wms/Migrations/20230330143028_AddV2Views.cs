using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    using System.Linq;

    public partial class AddV2Views : Migration
    {
        private readonly AddViews.StatusViews _buildingViews = new AddViews.StatusViews(
            "GebouwViewV2",
            new AddViews.StatusView { Name = "GebouwGeplandV2", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerdV2", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerdV2", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerdV2", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouwV2", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly AddViews.StatusViews _buildingUnitViews = new AddViews.StatusViews(
            "GebouweenheidViewV2",
            new AddViews.StatusView { Name = "GebouweenheidGehistoreerdV2", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouweenheidGeplandV2", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouweenheidGerealiseerdV2", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouweenheidNietGerealiseerdV2", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" }
        );

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateBuildingViews(migrationBuilder);
            CreateBuildingUnitViews(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            void Drop(AddViews.StatusViews statusViews)
            {
                var query = statusViews.Views
                    .Select(view => view.Name)
                    .Concat(new[] { _buildingUnitViews.SourceViewName })
                    .Aggregate("", (current, view) => current + $@"
                        DROP VIEW [wms].[{view}]
                        GO");

                migrationBuilder.Sql(query);
            }

            Drop(_buildingUnitViews);
            migrationBuilder.Sql(@"
                DROP INDEX [SPATIAL_GebouweenheidV2_Geometry] ON [wms].[buildingUnitsV2]
                GO

	            ALTER TABLE [wms].[buildingUnitsV2]
		            DROP COLUMN CalculatedGeometry
	            GO");

            Drop(_buildingViews);
            migrationBuilder.Sql(@"
                DROP INDEX [SPATIAL_GebouwV2_Geometrie] ON [wms].[buildingsV2]
                GO

                ALTER TABLE [wms].[buildingsV2]
		            DROP COLUMN CalculatedGeometry
	            GO");
        }

        private void CreateBuildingViews(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
	            ALTER TABLE [wms].[buildingsV2]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Geometry], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
	            CREATE SPATIAL INDEX [SPATIAL_GebouwV2_Geometrie] ON [wms].[buildingsV2] ([CalculatedGeometry])
	            USING  GEOMETRY_GRID
	            WITH (
		            BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
		            GRIDS =(
			            LEVEL_1 = MEDIUM,
			            LEVEL_2 = MEDIUM,
			            LEVEL_3 = MEDIUM,
			            LEVEL_4 = MEDIUM),
	            CELLS_PER_OBJECT = 5)
	            GO");

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
                FROM [wms].[BuildingsV2]
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
            migrationBuilder.Sql(@"
	            ALTER TABLE [wms].[buildingUnitsV2]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Position], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX [SPATIAL_GebouweenheidV2_Geometry] ON [wms].[buildingUnitsV2] ([CalculatedGeometry])
                USING  GEOMETRY_GRID
                WITH (
                    BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
                    GRIDS =(
                            LEVEL_1 = MEDIUM,
                            LEVEL_2 = MEDIUM,
                            LEVEL_3 = MEDIUM,
                            LEVEL_4 = MEDIUM),
                    CELLS_PER_OBJECT = 5
                )
                GO");

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
                FROM [wms].[buildingUnitsV2]                
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
    }
}
