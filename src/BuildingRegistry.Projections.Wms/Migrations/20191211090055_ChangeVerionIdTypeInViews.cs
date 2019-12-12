using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    using System.Collections.Generic;
    using Infrastructure;

    public partial class ChangeVerionIdTypeInViews : Migration
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
            AlterBuildingViews(migrationBuilder);
            AlterBuildingUnitViews(migrationBuilder);
        }

        private void AlterBuildingUnitViews(MigrationBuilder migrationBuilder)
        {
            DropViews(migrationBuilder, _buildingUnitViews);

            migrationBuilder.Sql($@"
                CREATE VIEW [{Schema.Wms}].[{_buildingUnitViews.SourceViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    Id,
                    BuildingUnitPersistentLocalId AS ObjectId,
                    CAST([Version] AS nvarchar) AS VersieId,
                    PositionMethod AS PositieGeometrieMethode,
                    [Status] AS GebouweenheidStatus,
                    [Function] AS Functie,
                    BuildingPersistentLocalId AS GebouwObjectId,
                    geometry::STGeomFromWKB([Position], 0) AS Geometrie
                FROM {Schema.Wms}.{BuildingUnit.BuildingUnitConfiguration.TableName}
                WHERE (IsComplete = 1) AND (IsBuildingComplete = 1)
                GO");

            foreach (var view1 in _buildingUnitViews.Views)
                migrationBuilder.Sql($@"
                    CREATE VIEW [{Schema.Wms}].[{view1.Name}]
                    WITH SCHEMABINDING
                    AS
                    SELECT
                        Id,
                        ObjectId,
                        VersieId,
                        PositieGeometrieMethode,
                        '{view1.DisplayedStatus}' as GebouweenheidStatus,
                        Functie,
                        GebouwObjectId,
                        Geometrie as [Geometry]
                    FROM {Schema.Wms}.{_buildingUnitViews.SourceViewName}
                    WHERE GebouweenheidStatus = '{view1.Criteria}'
                    GO");
        }

        private void AlterBuildingViews(MigrationBuilder migrationBuilder)
        {
            DropViews(migrationBuilder, _buildingViews);

            migrationBuilder.Sql($@"
                CREATE VIEW [{Schema.Wms}].[{_buildingViews.SourceViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    PersistentLocalId AS ObjectId,
                    Id,
                    CAST([Version] AS nvarchar) AS [Version],
                    geometry::STGeomFromWKB([Geometry], 0) AS Geometry,
                    GeometryMethod AS GeometrieMethode,
                    [Status]
                FROM {Schema.Wms}.{Building.BuildingConfiguration.TableName}
                WHERE (IsComplete = 1)
                GO");

            foreach (var view in _buildingViews.Views)
                migrationBuilder.Sql($@"
                    CREATE VIEW [{Schema.Wms}].[{view.Name}]
                    WITH SCHEMABINDING
                    AS
                    SELECT
                        ObjectId,
                        Id,
                        [Version] as VersieId,
                        Geometry,
                        GeometrieMethode,
                        '{view.DisplayedStatus}' as GebouwStatus
                    FROM {Schema.Wms}.{_buildingViews.SourceViewName}
                    WHERE Status = '{view.Criteria}'
                    GO");
        }

        private static void DropViews(MigrationBuilder migrationBuilder, StatusViews status)
        {
            void Drop(string name) => migrationBuilder.Sql($@"
                DROP VIEW [{Schema.Wms}].[{name}]
                GO");

            foreach (var view in status.Views)
                Drop(view.Name);
            Drop(status.SourceViewName);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            DropViews(migrationBuilder, _buildingViews);
            DropViews(migrationBuilder, _buildingUnitViews);

            // restore previous version of the views
            migrationBuilder.Sql($@"
            CREATE VIEW [{Schema.Wms}].[GebouwView]
            WITH SCHEMABINDING
            AS
            SELECT        PersistentLocalId AS ObjectId, Id, Version, geometry::STGeomFromWKB([Geometry], 0) AS Geometry, GeometryMethod AS GeometrieMethode, Status
            FROM            {Schema.Wms}.{Building.BuildingConfiguration.TableName}
            WHERE        (IsComplete = 1)
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwGepland]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'Gepland' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'Planned'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwGehistoreerd]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'Gehistoreerd' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'Retired'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'Gerealiseerd' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'Realized'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwNietGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'NietGerealiseerd' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'NotRealized'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouwInAanbouw]
            WITH SCHEMABINDING
            AS
            SELECT        ObjectId, Id, Version as VersieId, Geometry, GeometrieMethode, 'InAanbouw' as GebouwStatus
            FROM            {Schema.Wms}.GebouwView
            WHERE         Status = 'UnderConstruction'
            GO");

            migrationBuilder.Sql($@"
            CREATE VIEW [{Schema.Wms}].[GebouweenheidView]
            WITH SCHEMABINDING
            AS
            SELECT        Id, BuildingUnitPersistentLocalId AS ObjectId, Version AS VersieId, PositionMethod AS PositieGeometrieMethode, Status AS GebouweenheidStatus, [Function] AS Functie, BuildingPersistentLocalId AS GebouwObjectId, geometry::STGeomFromWKB([Position], 0) AS Geometrie
            FROM            {Schema.Wms}.{BuildingUnit.BuildingUnitConfiguration.TableName}
            WHERE        (IsComplete = 1) AND (IsBuildingComplete = 1)
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidGehistoreerd]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'Gehistoreerd' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'Retired'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidGepland]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'Gepland' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'Planned'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'Gerealiseerd' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'Realized'
            GO

            CREATE VIEW [{Schema.Wms}].[GebouweenheidNietGerealiseerd]
            WITH SCHEMABINDING
            AS
            SELECT        Id, ObjectId, VersieId, PositieGeometrieMethode, 'NietGerealiseerd' as GebouweenheidStatus, Functie, GebouwObjectId, Geometrie as [Geometry]
            FROM            {Schema.Wms}.GebouweenheidView
            WHERE        GebouweenheidStatus = 'NotRealized'
            GO
            ");
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
