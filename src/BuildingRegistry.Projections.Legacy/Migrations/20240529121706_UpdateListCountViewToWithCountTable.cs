using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateListCountViewToWithCountTable : Migration
    {
        private static string ViewSchema => Infrastructure.Schema.Legacy;

        private static string BuildingUnitDetailViewName => LegacyContext.BuildingUnitDetailV2ListCountViewName;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP VIEW [{ViewSchema}].[{BuildingUnitDetailViewName}]");

            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[{BuildingUnitDetailViewName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{BuildingUnitDetailV2WithCount.BuildingUnitDetailItemConfiguration.TableName}]
            WHERE [IsRemoved] = 0");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_{BuildingUnitDetailViewName}
                ON [{ViewSchema}].[{BuildingUnitDetailViewName}] (Count)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP VIEW [{ViewSchema}].[{BuildingUnitDetailViewName}]");

            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[{BuildingUnitDetailViewName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{BuildingUnitDetailV2.BuildingUnitDetailItemConfiguration.TableName}]
            WHERE [IsRemoved] = 0");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_{BuildingUnitDetailViewName}
                ON [{ViewSchema}].[{BuildingUnitDetailViewName}] (Count)");
        }
    }
}
