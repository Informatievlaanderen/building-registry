using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    using System.Collections.Generic;
    using Infrastructure;

    public partial class ShowOnlyBuildingingsWithGeometies : Migration
    {
        private readonly string _buildingSourceViewName = "GebouwView"; 
        private readonly IEnumerable<StatusView> _buildingViews = new []
        {
            new StatusView { Name = "GebouwGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new StatusView { Name = "GebouwGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new StatusView { Name = "GebouwGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new StatusView { Name = "GebouwNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new StatusView { Name = "GebouwInAanbouw", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        }; 

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropViews(migrationBuilder);

            migrationBuilder.Sql($@"
                CREATE VIEW [{Schema.Wms}].[{_buildingSourceViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    PersistentLocalId AS ObjectId,
                    Id,
                    CAST([Version] AS nvarchar) AS [Version],
                    geometry::STGeomFromWKB([Geometry], 0) AS [Geometry],
                    GeometryMethod AS GeometrieMethode,
                    [Status]
                FROM {Schema.Wms}.{Building.BuildingConfiguration.TableName}
                WHERE (IsComplete = 1)
                AND ([Geometry] IS NOT NULL)
                GO");

            CreateBuildingStatusViews(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropViews(migrationBuilder);

            migrationBuilder.Sql($@"
                CREATE VIEW [{Schema.Wms}].[{_buildingSourceViewName}]
                WITH SCHEMABINDING
                AS
                SELECT
                    PersistentLocalId AS ObjectId,
                    Id,
                    CAST([Version] AS nvarchar) AS [Version],
                    geometry::STGeomFromWKB([Geometry], 0) AS [Geometry],
                    GeometryMethod AS GeometrieMethode,
                    [Status]
                FROM {Schema.Wms}.{Building.BuildingConfiguration.TableName}
                WHERE (IsComplete = 1)
                GO");

            CreateBuildingStatusViews(migrationBuilder);
        }

        private void CreateBuildingStatusViews(MigrationBuilder migrationBuilder)
        {
            foreach (var view in _buildingViews)
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
                    FROM {Schema.Wms}.{_buildingSourceViewName}
                    WHERE Status = '{view.Criteria}'
                    GO");
        }

        private void DropViews(MigrationBuilder migrationBuilder)
        {
            void Drop(string name) => migrationBuilder.Sql($@"
                DROP VIEW [{Schema.Wms}].[{name}]
                GO");

            foreach (var view in _buildingViews)
                Drop(view.Name);
            Drop(_buildingSourceViewName);
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
