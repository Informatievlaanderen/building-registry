using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    using Infrastructure;

    public partial class AddViews : Migration
    {
        private const string BuildingViewName = "GebouwView";
        private const string BuildingUnitViewName = "GebouweenheidView";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                FROM [{Schema.Wfs}].[BuildingsV2]
                WHERE [IsRemoved] = 0 and Geometry is not null");

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
                    [BuildingPersistentLocalId] AS [GebouwObjectId]
                FROM [{Schema.Wfs}].[{BuildingUnitV2.BuildingUnitConfiguration.TableName}]
                WHERE [IsRemoved] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingViewName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingUnitViewName}]");
        }
    }
}
