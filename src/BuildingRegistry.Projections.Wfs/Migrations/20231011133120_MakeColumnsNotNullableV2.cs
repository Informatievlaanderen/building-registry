using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class MakeColumnsNotNullableV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "SPATIAL_BuildingUnitsV2_Position",
                schema: "wfs",
                table: "BuildingUnitsV2");

            migrationBuilder.AlterColumn<string>(
                name: "PositionMethod",
                schema: "wfs",
                table: "BuildingUnitsV2",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Geometry>(
                name: "Position",
                schema: "wfs",
                table: "BuildingUnitsV2",
                type: "sys.geometry",
                nullable: false,
                oldClrType: typeof(Geometry),
                oldType: "sys.geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GeometryMethod",
                schema: "wfs",
                table: "BuildingsV2",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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
            migrationBuilder.DropIndex(
                name: "SPATIAL_BuildingUnitsV2_Position",
                schema: "wfs",
                table: "BuildingUnitsV2");

            migrationBuilder.AlterColumn<string>(
                name: "PositionMethod",
                schema: "wfs",
                table: "BuildingUnitsV2",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Geometry>(
                name: "Position",
                schema: "wfs",
                table: "BuildingUnitsV2",
                type: "sys.geometry",
                nullable: true,
                oldClrType: typeof(Geometry),
                oldType: "sys.geometry");

            migrationBuilder.AlterColumn<string>(
                name: "GeometryMethod",
                schema: "wfs",
                table: "BuildingsV2",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
    }
}
