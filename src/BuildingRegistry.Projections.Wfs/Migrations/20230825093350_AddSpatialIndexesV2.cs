using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class AddSpatialIndexesV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
	            CREATE SPATIAL INDEX [SPATIAL_BuildingsV2_Geometry] ON [wfs].[buildingsV2] ([Geometry])
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
                CREATE SPATIAL INDEX [SPATIAL_BuildingUnitsV2_Position] ON [wfs].[buildingUnitsV2] ([Position])
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
                DROP INDEX [SPATIAL_BuildingUnitsV2_Position] ON [wfs].[buildingUnitsV2]
                GO");

            migrationBuilder.Sql(@"
                DROP INDEX [SPATIAL_BuildingsV2_Geometry] ON [wfs].[buildingsV2]
                GO");
        }
    }
}
