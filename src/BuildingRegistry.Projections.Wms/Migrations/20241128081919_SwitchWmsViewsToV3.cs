using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    /// <inheritdoc />
    public partial class SwitchWmsViewsToV3 : Migration
    {
        private readonly AddViews.StatusViews _buildingUpViews = new AddViews.StatusViews(
            "GebouwView",
            new AddViews.StatusView { Name = "GebouwGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouw", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly AddViews.StatusViews _buildingDownViews = new AddViews.StatusViews(
            "GebouwView",
            new AddViews.StatusView { Name = "GebouwGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouw", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwInAanbouw]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGepland]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwNietGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwView]");

            migrationBuilder.Sql($@"
                CREATE VIEW [wms].[{_buildingUpViews.SourceViewName}]
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
                FROM [wms].[{BuildingV3.BuildingConfiguration.TableName}]
                WHERE ([CalculatedGeometry] IS NOT NULL)
                GO");

            foreach (var view in _buildingUpViews.Views)
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
                    FROM [wms].[{_buildingUpViews.SourceViewName}]
                    WHERE Status = '{view.Criteria}'
                    GO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwInAanbouw]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGepland]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwNietGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwView]");

            migrationBuilder.Sql($@"
                CREATE VIEW [wms].[{_buildingDownViews.SourceViewName}]
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

            foreach (var view in _buildingDownViews.Views)
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
                    FROM [wms].[{_buildingDownViews.SourceViewName}]
                    WHERE Status = '{view.Criteria}'
                    GO");
        }
    }
}
