using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    using Infrastructure;

    /// <inheritdoc />
    public partial class SwitchWmsViewsToV3 : Migration
    {
        private const string BuildingViewName = "GebouwView";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingViewName}]");

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
                FROM [{Schema.Wfs}].[{BuildingV3.BuildingConfiguration.TableName}]
                WHERE [IsRemoved] = 0 and Geometry is not null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Schema.Wfs}].[{BuildingViewName}]");

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
        }
    }
}
