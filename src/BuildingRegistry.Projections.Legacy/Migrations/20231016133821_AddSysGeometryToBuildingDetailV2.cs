using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    using BuildingDetailV2;
    using Infrastructure;

    public partial class AddSysGeometryToBuildingDetailV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Geometry>(
                name: "SysGeometry",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                type: "sys.geometry",
                nullable: true);

            migrationBuilder.Sql(@$"CREATE SPATIAL INDEX [SPATIAL_BuildingDetailsV2_Geometry] ON [{Schema.Legacy}].[{BuildingDetailItemConfiguration.TableName}] ([SysGeometry])
         USING GEOMETRY_GRID
         WITH (
          BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
          GRIDS =(
           LEVEL_1 = MEDIUM,
           LEVEL_2 = MEDIUM,
           LEVEL_3 = MEDIUM,
           LEVEL_4 = MEDIUM),
         CELLS_PER_OBJECT = 5)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"
            DROP INDEX [SPATIAL_BuildingDetailsV2_Geometry] ON [{Schema.Legacy}].[{BuildingDetailItemConfiguration.TableName}]");

            migrationBuilder.DropColumn(
                name: "SysGeometry",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2");
        }
    }
}
