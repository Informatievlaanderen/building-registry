using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class ChangeVersieType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "BuildingRegistrySyndication",
                table: "BuildingParcelLatestItems",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "BuildingRegistrySyndication",
                table: "BuildingParcelLatestItems",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "BuildingRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
