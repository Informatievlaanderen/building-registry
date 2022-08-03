using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddV2ListCountViews : Migration
    {
        private static string ViewSchema => Infrastructure.Schema.Legacy;

        private static string BuildingDetailViewName => LegacyContext.BuildingDetailV2ListCountViewName;
        private static string BuildingUnitDetailViewName => LegacyContext.BuildingUnitDetailV2ListCountViewName;


        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateBuildingListCountView(migrationBuilder);
            CreateBuildingUnitListCountView(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropListCountView(migrationBuilder, BuildingDetailViewName);
            DropListCountView(migrationBuilder, BuildingUnitDetailViewName);
        }

        private static void CreateBuildingListCountView(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[{BuildingDetailViewName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{BuildingDetailV2.BuildingDetailItemConfiguration.TableName}]
            WHERE [IsRemoved] = 0");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_{BuildingDetailViewName}
                ON [{ViewSchema}].[{BuildingDetailViewName}] (Count)");
        }

        private static void CreateBuildingUnitListCountView(MigrationBuilder migrationBuilder)
        {
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
        private static void DropListCountView(MigrationBuilder migrationBuilder, string viewName)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{viewName}] ON [{ViewSchema}].[{viewName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{viewName}]");
        }
    }
}
