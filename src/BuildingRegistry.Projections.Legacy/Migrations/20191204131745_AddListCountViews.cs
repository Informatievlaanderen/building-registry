using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddListCountViews : Migration
    {
        private static string ViewSchema => Infrastructure.Schema.Legacy;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateBuildingListCountView(migrationBuilder);
            CreateBuildingUnitListCountView(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropListCountView(migrationBuilder, LegacyContext.BuildingDetailListCountViewName);
            DropListCountView(migrationBuilder, LegacyContext.BuildingUnitDetailListCountViewName);
        }

        private static void CreateBuildingListCountView(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[{LegacyContext.BuildingDetailListCountViewName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{BuildingDetail.BuildingDetailItemConfiguration.TableName}]
            WHERE [IsComplete] = 1
                AND [IsRemoved] = 0
                AND [PersistentLocalId] IS NOT NULL");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.BuildingDetailListCountViewName}
                ON [{ViewSchema}].[{LegacyContext.BuildingDetailListCountViewName}] (Count)");
        }

        private static void CreateBuildingUnitListCountView(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[{LegacyContext.BuildingUnitDetailListCountViewName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{BuildingUnitDetail.BuildingUnitDetailItemConfiguration.TableName}]
            WHERE [IsComplete] = 1
                AND [IsRemoved] = 0
                AND [PersistentLocalId] IS NOT NULL
                AND [IsBuildingComplete] = 1
                AND [BuildingPersistentLocalId] IS NOT NULL");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.BuildingUnitDetailListCountViewName}
                ON [{ViewSchema}].[{LegacyContext.BuildingUnitDetailListCountViewName}] (Count)");
        }
        private static void DropListCountView(MigrationBuilder migrationBuilder, string viewName)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{viewName}] ON [{ViewSchema}].[{viewName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{viewName}]");
        }
    }
}
