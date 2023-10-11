using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class MakeColumnsNotNullableV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
