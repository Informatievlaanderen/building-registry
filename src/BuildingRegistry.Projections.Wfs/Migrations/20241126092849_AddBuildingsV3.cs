using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingsV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV3",
                schema: "wfs",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: true),
                    GeometryMethod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV3", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_GeometryMethod",
                schema: "wfs",
                table: "BuildingsV3",
                column: "GeometryMethod");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_Id",
                schema: "wfs",
                table: "BuildingsV3",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_IsRemoved",
                schema: "wfs",
                table: "BuildingsV3",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_PersistentLocalId",
                schema: "wfs",
                table: "BuildingsV3",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_Status",
                schema: "wfs",
                table: "BuildingsV3",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV3_VersionAsString",
                schema: "wfs",
                table: "BuildingsV3",
                column: "VersionAsString");

            migrationBuilder.Sql(@"
	            CREATE SPATIAL INDEX [SPATIAL_BuildingsV3_Geometry] ON [wfs].[buildingsV3] ([Geometry])
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
                schema: "wfs");
        }
    }
}
