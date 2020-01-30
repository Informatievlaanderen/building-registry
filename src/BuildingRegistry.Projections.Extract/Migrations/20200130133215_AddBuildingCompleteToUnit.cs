using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddBuildingCompleteToUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "DbaseRecord",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBuildingComplete",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ShapeRecordContent",
                schema: "BuildingRegistryExtract",
                table: "Building",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "DbaseRecord",
                schema: "BuildingRegistryExtract",
                table: "Building",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_IsComplete_IsBuildingComplete",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                columns: new[] { "IsComplete", "IsBuildingComplete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnit_PersistentLocalId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnit_IsComplete_IsBuildingComplete",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.DropColumn(
                name: "IsBuildingComplete",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DbaseRecord",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                nullable: true,
                oldClrType: typeof(byte[]));

            migrationBuilder.AlterColumn<byte[]>(
                name: "ShapeRecordContent",
                schema: "BuildingRegistryExtract",
                table: "Building",
                nullable: true,
                oldClrType: typeof(byte[]));

            migrationBuilder.AlterColumn<byte[]>(
                name: "DbaseRecord",
                schema: "BuildingRegistryExtract",
                table: "Building",
                nullable: true,
                oldClrType: typeof(byte[]));
        }
    }
}
