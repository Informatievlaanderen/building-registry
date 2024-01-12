using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_building");

            migrationBuilder.CreateTable(
                name: "building_latest_items",
                schema: "integration_building",
                columns: table => new
                {
                    building_persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "text", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
                    geometry_method = table.Column<string>(type: "text", nullable: false),
                    oslo_geometry_method = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    nis_code = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_latest_items", x => x.building_persistent_local_id);
                });

            migrationBuilder.CreateTable(
                name: "building_unit_addresses",
                schema: "integration_building",
                columns: table => new
                {
                    building_unit_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    address_persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_addresses", x => new { x.building_unit_persistent_local_id, x.address_persistent_local_id });
                });

            migrationBuilder.CreateTable(
                name: "building_unit_latest_items",
                schema: "integration_building",
                columns: table => new
                {
                    building_unit_persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    building_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
                    function = table.Column<string>(type: "text", nullable: false),
                    oslo_function = table.Column<string>(type: "text", nullable: false),
                    geometry_method = table.Column<string>(type: "text", nullable: false),
                    oslo_geometry_method = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    has_deviation = table.Column<bool>(type: "boolean", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_latest_items", x => x.building_unit_persistent_local_id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_building",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "text", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_building_latest_items_geometry",
                schema: "integration_building",
                table: "building_latest_items",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_building_latest_items_is_removed",
                schema: "integration_building",
                table: "building_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_building_latest_items_nis_code",
                schema: "integration_building",
                table: "building_latest_items",
                column: "nis_code");

            migrationBuilder.CreateIndex(
                name: "IX_building_latest_items_oslo_status",
                schema: "integration_building",
                table: "building_latest_items",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_building_latest_items_status",
                schema: "integration_building",
                table: "building_latest_items",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_addresses_address_persistent_local_id",
                schema: "integration_building",
                table: "building_unit_addresses",
                column: "address_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_addresses_building_unit_persistent_local_id",
                schema: "integration_building",
                table: "building_unit_addresses",
                column: "building_unit_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_latest_items_building_persistent_local_id",
                schema: "integration_building",
                table: "building_unit_latest_items",
                column: "building_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_latest_items_geometry",
                schema: "integration_building",
                table: "building_unit_latest_items",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_latest_items_is_removed",
                schema: "integration_building",
                table: "building_unit_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_latest_items_oslo_status",
                schema: "integration_building",
                table: "building_unit_latest_items",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_latest_items_status",
                schema: "integration_building",
                table: "building_unit_latest_items",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "building_latest_items",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_addresses",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_latest_items",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_building");
        }
    }
}
