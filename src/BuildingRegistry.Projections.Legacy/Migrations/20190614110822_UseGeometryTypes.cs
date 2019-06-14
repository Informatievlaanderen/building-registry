using System;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class UseGeometryTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionWkbHex",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication");

            migrationBuilder.DropColumn(
                name: "GeometryWkbHex",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication");

            migrationBuilder.AddColumn<IPoint>(
                name: "PointPosition",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication",
                nullable: true);

            migrationBuilder.AlterColumn<IPoint>(
                name: "Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AddColumn<IPolygon>(
                name: "Geometry",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                nullable: true);

            migrationBuilder.AlterColumn<IPolygon>(
                name: "Geometry",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointPosition",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication");

            migrationBuilder.DropColumn(
                name: "Geometry",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication");

            migrationBuilder.AddColumn<string>(
                name: "PositionWkbHex",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndication",
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Position",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                nullable: true,
                oldClrType: typeof(IPoint),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeometryWkbHex",
                schema: "BuildingRegistryLegacy",
                table: "BuildingSyndication",
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Geometry",
                schema: "BuildingRegistryLegacy",
                table: "BuildingDetails",
                nullable: true,
                oldClrType: typeof(IPolygon),
                oldNullable: true);
        }
    }
}
