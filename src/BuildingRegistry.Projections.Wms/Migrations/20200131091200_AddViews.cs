namespace BuildingRegistry.Projections.Wms.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;
    using System.Collections.Generic;
    using System.Linq;

    public partial class AddViews : Migration
    {
        private readonly StatusViews _buildingViews = new StatusViews(
            "GebouwView",
            new StatusView { Name = "GebouwGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new StatusView { Name = "GebouwGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new StatusView { Name = "GebouwGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new StatusView { Name = "GebouwNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new StatusView { Name = "GebouwInAanbouw", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly StatusViews _buildingUnitViews = new StatusViews(
            "GebouweenheidView",
            new StatusView { Name = "GebouweenheidGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new StatusView { Name = "GebouweenheidGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new StatusView { Name = "GebouweenheidGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new StatusView { Name = "GebouweenheidNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" }
        );


        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateBuildingViews(migrationBuilder);
            CreateBuildingUnitViews(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            void Drop(StatusViews statusViews)
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
                DROP INDEX [SPATIAL_Gebouweenheid_Geometry] ON [wms].[buildingUnits]
                GO

	            ALTER TABLE [wms].[buildingUnits]
		            DROP COLUMN CalculatedGeometry
	            GO");

            Drop(_buildingViews);
            migrationBuilder.Sql(@"
                DROP INDEX [SPATIAL_Gebouw_Geometrie] ON [wms].[buildings]
                GO

                ALTER TABLE [wms].[buildings]
		            DROP COLUMN CalculatedGeometry
	            GO");
        }

        private void CreateBuildingViews(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
	            ALTER TABLE [wms].[buildings]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Geometry], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
	            CREATE SPATIAL INDEX [SPATIAL_Gebouw_Geometrie] ON [wms].[buildings] ([CalculatedGeometry])
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
                FROM [wms].[Buildings]
                WHERE (IsComplete = 1)
                    AND ([CalculatedGeometry] IS NOT NULL)
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
	            ALTER TABLE [wms].[buildingUnits]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Position], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX [SPATIAL_Gebouweenheid_Geometry] ON [wms].[buildingUnits] ([CalculatedGeometry])
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
                    [Version] AS RawVersion
                FROM [wms].[buildingUnits]
                WHERE (IsComplete = 1) AND (IsBuildingComplete = 1)
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
                        [GebouwObjectId],
                        [Geometry]
                    FROM [wms].[{_buildingUnitViews.SourceViewName}]
                    WHERE GebouweenheidStatus = '{view.Criteria}'
                    GO");
        }

        private class StatusViews
        {
            public StatusViews(string sourceViewName, params StatusView[] views)
            {
                SourceViewName = sourceViewName;
                Views = views;
            }

            public string SourceViewName { get; }
            public IEnumerable<StatusView> Views { get; }
        }

        private class StatusView
        {
            public string Name { get; set; }
            public string DisplayedStatus { get; set; }
            public string Criteria { get; set; }

        }
    }
}
