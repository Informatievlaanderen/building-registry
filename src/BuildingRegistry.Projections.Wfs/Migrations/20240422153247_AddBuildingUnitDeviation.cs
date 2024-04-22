using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    using Infrastructure;

    /// <inheritdoc />
    public partial class AddBuildingUnitDeviation : Migration
    {
        private const string BuildingUnitViewName = "GebouweenheidView";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingUnitViewName}]");
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
                FROM [{Schema.Wfs}].[{BuildingUnitV2.BuildingUnitConfiguration.TableName}]
                WHERE [IsRemoved] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingUnitViewName}]");
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
    }
}
