using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingsV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV3",
                schema: "wms",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "varchar(46)", maxLength: 46, nullable: true),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV3", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_Status",
                schema: "wms",
                table: "BuildingsV3",
                column: "Status");

            migrationBuilder.Sql(@"
	            ALTER TABLE [wms].[BuildingsV3]
		            ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Geometry], 31370)) PERSISTED
	            GO");

            migrationBuilder.Sql(@"
	            CREATE SPATIAL INDEX [SPATIAL_GebouwV3_Geometrie] ON [wms].[BuildingsV3] ([CalculatedGeometry])
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingsV3",
                schema: "wms");
        }
    }
}
