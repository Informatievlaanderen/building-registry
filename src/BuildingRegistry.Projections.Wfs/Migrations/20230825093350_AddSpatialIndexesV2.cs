using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class AddSpatialIndexesV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
	            ALTER TABLE [wfs].[buildingsV2]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Geometry], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
	            CREATE SPATIAL INDEX [SPATIAL_GebouwV2_Geometrie] ON [wfs].[buildingsV2] ([CalculatedGeometry])
	            USING  GEOMETRY_GRID
	            WITH (
		            BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
		            GRIDS =(
			            LEVEL_1 = MEDIUM,
			            LEVEL_2 = MEDIUM,
			            LEVEL_3 = MEDIUM,
			            LEVEL_4 = MEDIUM),
	            CELLS_PER_OBJECT = 5)
	            GO");

            migrationBuilder.Sql(@"
	            ALTER TABLE [wfs].[buildingUnitsV2]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Position], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX [SPATIAL_GebouweenheidV2_Geometry] ON [wfs].[buildingUnitsV2] ([CalculatedGeometry])
                USING  GEOMETRY_GRID
                WITH (
                    BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
                    GRIDS =(
                            LEVEL_1 = MEDIUM,
                            LEVEL_2 = MEDIUM,
                            LEVEL_3 = MEDIUM,
                            LEVEL_4 = MEDIUM),
                    CELLS_PER_OBJECT = 5
                )
                GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX [SPATIAL_GebouweenheidV2_Geometry] ON [wfs].[buildingUnitsV2]
                GO

	            ALTER TABLE [wfs].[buildingUnitsV2]
		            DROP COLUMN CalculatedGeometry
	            GO");

            migrationBuilder.Sql(@"
                DROP INDEX [SPATIAL_GebouwV2_Geometrie] ON [wfs].[buildingsV2]
                GO

                ALTER TABLE [wfs].[buildingsV2]
		            DROP COLUMN CalculatedGeometry
	            GO");
        }
    }
}
