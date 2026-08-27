using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Consumer.Read.Parcel.Migrations
{
    using BuildingRegistry.Infrastructure;
    using ParcelWithCount;

    /// <inheritdoc />
    public partial class AddGeometryLambert2008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Geometry>(
                name: "GeometryLambert2008",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItemsWithCount",
                type: "sys.geometry",
                nullable: true);

            // The Lambert 72 bounding box of SPATIAL_ParcelItems_Geometry expressed in Lambert 2008: all four
            // corners transformed, then the envelope padded out to the next 100 m. Lambert 2008 coordinates
            // fall entirely outside the Lambert 72 box, so the two indexes cannot share one. See ADR 0006.
            migrationBuilder.Sql(@$"CREATE SPATIAL INDEX [SPATIAL_ParcelItems_GeometryLambert2008] ON [{Schema.ConsumerReadParcel}].[{ParcelConsumerItemConfiguration.TableName}] ([GeometryLambert2008])
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
            DROP INDEX [SPATIAL_ParcelItems_GeometryLambert2008] ON [{Schema.ConsumerReadParcel}].[{ParcelConsumerItemConfiguration.TableName}]");

            migrationBuilder.DropColumn(
                name: "GeometryLambert2008",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItemsWithCount");
        }
    }
}
