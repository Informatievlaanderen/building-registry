using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class SwitchWmsViews : Migration
    {
        private readonly AddViews.StatusViews _buildingUpViews = new AddViews.StatusViews(
            "GebouwView",
            new AddViews.StatusView { Name = "GebouwGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouw", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly AddViews.StatusViews _buildingUpUnitViews = new AddViews.StatusViews(
            "GebouweenheidView",
            new AddViews.StatusView { Name = "GebouweenheidGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouweenheidGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouweenheidGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouweenheidNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" }
        );

        private readonly AddViews.StatusViews _buildingDownViewsV2 = new AddViews.StatusViews(
            "GebouwViewV2",
            new AddViews.StatusView { Name = "GebouwGeplandV2", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerdV2", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerdV2", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerdV2", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouwV2", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly AddViews.StatusViews _buildingUnitDownViewsV2 = new AddViews.StatusViews(
            "GebouweenheidViewV2",
            new AddViews.StatusView { Name = "GebouweenheidGehistoreerdV2", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouweenheidGeplandV2", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouweenheidGerealiseerdV2", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouweenheidNietGerealiseerdV2", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" }
        );

        private readonly AddViews.StatusViews _buildingDownViews = new AddViews.StatusViews(
            "GebouwView",
            new AddViews.StatusView { Name = "GebouwGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouwGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouwGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouwNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" },
            new AddViews.StatusView { Name = "GebouwInAanbouw", DisplayedStatus = "InAanbouw", Criteria = "UnderConstruction" }
        );

        private readonly AddViews.StatusViews _buildingUnitDownViews = new AddViews.StatusViews(
            "GebouweenheidView",
            new AddViews.StatusView { Name = "GebouweenheidGehistoreerd", DisplayedStatus = "Gehistoreerd", Criteria = "Retired" },
            new AddViews.StatusView { Name = "GebouweenheidGepland", DisplayedStatus = "Gepland", Criteria = "Planned" },
            new AddViews.StatusView { Name = "GebouweenheidGerealiseerd", DisplayedStatus = "Gerealiseerd", Criteria = "Realized" },
            new AddViews.StatusView { Name = "GebouweenheidNietGerealiseerd", DisplayedStatus = "NietGerealiseerd", Criteria = "NotRealized" }
        );

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGehistoreerdV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGepland]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGeplandV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGerealiseerdV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidNietGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidNietGerealiseerdV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidView]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidViewV2]");

            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGehistoreerdV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwInAanbouw]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwInAanbouwV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGepland]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGeplandV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGerealiseerdV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwNietGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwNietGerealiseerdV2]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwView]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwViewV2]");

            CreateBuildingViews(migrationBuilder, _buildingUpViews, BuildingV2.BuildingConfiguration.TableName);
            CreateBuildingUnitViews(migrationBuilder, _buildingUpUnitViews, BuildingUnitV2.BuildingUnitConfiguration.TableName);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGepland]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidNietGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouweenheidView]");

            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwInAanbouw]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGepland]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwNietGerealiseerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[GebouwView]");

            CreateBuildingViews(migrationBuilder, _buildingDownViews, Building.BuildingConfiguration.TableName);
            CreateBuildingUnitViews(migrationBuilder, _buildingUnitDownViews, BuildingUnit.BuildingUnitConfiguration.TableName);

            CreateBuildingViews(migrationBuilder, _buildingDownViewsV2, BuildingV2.BuildingConfiguration.TableName);
            CreateBuildingUnitViews(migrationBuilder, _buildingUnitDownViewsV2, BuildingUnitV2.BuildingUnitConfiguration.TableName);
        }

        private void CreateBuildingViews(MigrationBuilder migrationBuilder, AddViews.StatusViews buildingViews, string sourceTableName)
        {
            migrationBuilder.Sql($@"
                CREATE VIEW [wms].[{buildingViews.SourceViewName}]
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
                FROM [wms].[{sourceTableName}]
                WHERE ([CalculatedGeometry] IS NOT NULL)
                GO");

            foreach (var view in buildingViews.Views)
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
                    FROM [wms].[{buildingViews.SourceViewName}]
                    WHERE Status = '{view.Criteria}'
                    GO");
        }

        private void CreateBuildingUnitViews(MigrationBuilder migrationBuilder, AddViews.StatusViews buildingUnitViews, string sourceTableName)
        {
            migrationBuilder.Sql($@"
                CREATE VIEW [wms].[{buildingUnitViews.SourceViewName}]
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
                FROM [wms].[{sourceTableName}]
                GO");

            foreach (var view in buildingUnitViews.Views)
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
                    FROM [wms].[{buildingUnitViews.SourceViewName}]
                    WHERE GebouweenheidStatus = '{view.Criteria}'
                    GO");
        }
    }
}
