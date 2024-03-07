using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    public partial class ReaddressOldAddressId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_building_unit_readdress_versions",
                schema: "integration_building",
                table: "building_unit_readdress_versions");

            migrationBuilder.AddColumn<Guid>(
                name: "old_address_id",
                schema: "integration_building",
                table: "building_unit_readdress_versions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_building_unit_readdress_versions",
                schema: "integration_building",
                table: "building_unit_readdress_versions",
                columns: new[] { "position", "building_unit_persistent_id", "old_address_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_building_unit_readdress_versions",
                schema: "integration_building",
                table: "building_unit_readdress_versions");

            migrationBuilder.DropColumn(
                name: "old_address_id",
                schema: "integration_building",
                table: "building_unit_readdress_versions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_building_unit_readdress_versions",
                schema: "integration_building",
                table: "building_unit_readdress_versions",
                columns: new[] { "position", "building_unit_persistent_id", "new_address_id" });
        }
    }
}
