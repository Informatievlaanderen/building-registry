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
            DropListCountView(migrationBuilder, "vw_BuildingDetailListCountView");
            DropListCountView(migrationBuilder, "vw_BuildingUnitDetailListCountView");
        }

        private static void CreateBuildingListCountView(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[vw_BuildingDetailListCountView]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[BuildingDetails]
            WHERE [IsComplete] = 1
                AND [IsRemoved] = 0
                AND [PersistentLocalId] IS NOT NULL");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_vw_BuildingDetailListCountView
                ON [{ViewSchema}].[vw_BuildingDetailListCountView] (Count)");
        }

        private static void CreateBuildingUnitListCountView(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{ViewSchema}].[vw_BuildingUnitDetailListCountView]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[BuildingUnitDetails]
            WHERE [IsComplete] = 1
                AND [IsRemoved] = 0
                AND [PersistentLocalId] IS NOT NULL
                AND [IsBuildingComplete] = 1
                AND [BuildingPersistentLocalId] IS NOT NULL");

            migrationBuilder.Sql($@"
            CREATE UNIQUE CLUSTERED INDEX IX_vw_BuildingUnitDetailListCountView
                ON [{ViewSchema}].[vw_BuildingUnitDetailListCountView] (Count)");
        }
        private static void DropListCountView(MigrationBuilder migrationBuilder, string viewName)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{viewName}] ON [{ViewSchema}].[{viewName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{viewName}]");
        }
    }
}
