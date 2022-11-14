﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    public partial class AddBuildingUnitHasDeviation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasDeviation",
                schema: "wfs",
                table: "BuildingUnitsV2",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDeviation",
                schema: "wfs",
                table: "BuildingUnitsV2");
        }
    }
}
