using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    using BuildingDetailV2;
    using Infrastructure;

    /// <inheritdoc />
    public partial class AddSysGeometryLambert2008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Geometry>(
                name: "SysGeometryLambert2008",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2",
                type: "sys.geometry",
                nullable: true);

            // The Lambert 72 bounding box of SPATIAL_BuildingDetailsV2_Geometry expressed in Lambert 2008:
            // all four corners transformed, then the envelope padded out to the next 100 m. Lambert 2008
            // coordinates fall entirely outside the Lambert 72 box, so the two indexes cannot share one -
            // an outline written after the conversion would get no useful index coverage from the old one.
            // The same numbers the parcel consumer and the WFS/WMS V4 tables use. See ADR 0006.
            migrationBuilder.Sql(@$"CREATE SPATIAL INDEX [SPATIAL_BuildingDetailsV2_GeometryLambert2008] ON [{Schema.Legacy}].[{BuildingDetailItemConfiguration.TableName}] ([SysGeometryLambert2008])
         USING GEOMETRY_GRID
         WITH (
          BOUNDING_BOX =(522200, 653000, 758900, 744100),
          GRIDS =(
           LEVEL_1 = MEDIUM,
           LEVEL_2 = MEDIUM,
           LEVEL_3 = MEDIUM,
           LEVEL_4 = MEDIUM),
         CELLS_PER_OBJECT = 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"
            DROP INDEX [SPATIAL_BuildingDetailsV2_GeometryLambert2008] ON [{Schema.Legacy}].[{BuildingDetailItemConfiguration.TableName}]");

            migrationBuilder.DropColumn(
                name: "SysGeometryLambert2008",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetailsV2");
        }
    }
}
