using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionsFromMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "building_versions_migration",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    geometry_method = table.Column<string>(type: "text", nullable: false),
                    oslo_geometry_method = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_as_string = table.Column<string>(type: "text", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_changed_on_as_string = table.Column<string>(type: "text", nullable: false),
                    last_changed_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_versions_migration", x => x.position);
                });

            migrationBuilder.CreateTable(
                name: "building_unit_versions_migration",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_unit_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    building_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
                    function = table.Column<string>(type: "text", nullable: false),
                    oslo_function = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    geometry_method = table.Column<string>(type: "text", nullable: false),
                    oslo_geometry_method = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    has_deviation = table.Column<bool>(type: "boolean", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_as_string = table.Column<string>(type: "text", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_versions_migration", x => new { x.position, x.building_unit_persistent_local_id });
                    table.ForeignKey(
                        name: "FK_building_unit_versions_migration_building_versions_migratio~",
                        column: x => x.position,
                        principalSchema: "integration_building",
                        principalTable: "building_versions_migration",
                        principalColumn: "position",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "building_unit_address_versions_migration",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_unit_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    address_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_address_versions_migration", x => new { x.position, x.building_unit_persistent_local_id, x.address_persistent_local_id });
                    table.ForeignKey(
                        name: "FK_building_unit_address_versions_migration_building_unit_vers~",
                        columns: x => new { x.position, x.building_unit_persistent_local_id },
                        principalSchema: "integration_building",
                        principalTable: "building_unit_versions_migration",
                        principalColumns: new[] { "position", "building_unit_persistent_local_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_address_versions_migration_address_persistent~",
                schema: "integration_building",
                table: "building_unit_address_versions_migration",
                column: "address_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_address_versions_migration_building_unit_pers~",
                schema: "integration_building",
                table: "building_unit_address_versions_migration",
                column: "building_unit_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_address_versions_migration_position",
                schema: "integration_building",
                table: "building_unit_address_versions_migration",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_building_persistent_local_~",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "building_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_building_unit_persistent_l~",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "building_unit_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_geometry",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_is_removed",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_oslo_status",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_status",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_type",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_migration_version_timestamp",
                schema: "integration_building",
                table: "building_unit_versions_migration",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_building_persistent_local_id",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "building_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_geometry",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_is_removed",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_oslo_status",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_status",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_type",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_migration_version_timestamp",
                schema: "integration_building",
                table: "building_versions_migration",
                column: "version_timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "building_unit_address_versions_migration",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_versions_migration",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_versions_migration",
                schema: "integration_building");
        }
    }
}
