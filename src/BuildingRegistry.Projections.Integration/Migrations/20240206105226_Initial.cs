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
                    puri = table.Column<string>(type: "text", nullable: false),
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
                    puri = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_latest_items", x => x.building_unit_persistent_local_id);
                });

            migrationBuilder.CreateTable(
                name: "building_versions",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_id = table.Column<Guid>(type: "uuid", nullable: true),
                    building_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    oslo_status = table.Column<string>(type: "text", nullable: true),
                    geometry_method = table.Column<string>(type: "text", nullable: true),
                    oslo_geometry_method = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: true),
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
                    table.PrimaryKey("PK_building_versions", x => x.position);
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

            migrationBuilder.CreateTable(
                name: "building_unit_versions",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_unit_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    building_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    building_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    oslo_status = table.Column<string>(type: "text", nullable: true),
                    function = table.Column<string>(type: "text", nullable: false),
                    oslo_function = table.Column<string>(type: "text", nullable: false),
                    geometry_method = table.Column<string>(type: "text", nullable: true),
                    oslo_geometry_method = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: true),
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
                    table.PrimaryKey("PK_building_unit_versions", x => new { x.position, x.building_unit_persistent_local_id });
                    table.ForeignKey(
                        name: "FK_building_unit_versions_building_versions_position",
                        column: x => x.position,
                        principalSchema: "integration_building",
                        principalTable: "building_versions",
                        principalColumn: "position",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "building_unit_address_versions",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_unit_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    address_persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_address_versions", x => new { x.position, x.building_unit_persistent_local_id, x.address_persistent_local_id });
                    table.ForeignKey(
                        name: "FK_building_unit_address_versions_building_unit_versions_posit~",
                        columns: x => new { x.position, x.building_unit_persistent_local_id },
                        principalSchema: "integration_building",
                        principalTable: "building_unit_versions",
                        principalColumns: new[] { "position", "building_unit_persistent_local_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "building_unit_readdress_versions",
                schema: "integration_building",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    building_unit_persistent_id = table.Column<int>(type: "integer", nullable: false),
                    new_address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    readdress_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_unit_readdress_versions", x => new { x.position, x.building_unit_persistent_id, x.new_address_id });
                    table.ForeignKey(
                        name: "FK_building_unit_readdress_versions_building_unit_versions_pos~",
                        columns: x => new { x.position, x.building_unit_persistent_id },
                        principalSchema: "integration_building",
                        principalTable: "building_unit_versions",
                        principalColumns: new[] { "position", "building_unit_persistent_local_id" },
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_building_unit_address_versions_address_persistent_local_id",
                schema: "integration_building",
                table: "building_unit_address_versions",
                column: "address_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_address_versions_building_unit_persistent_loc~",
                schema: "integration_building",
                table: "building_unit_address_versions",
                column: "building_unit_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_address_versions_position",
                schema: "integration_building",
                table: "building_unit_address_versions",
                column: "position");

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

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_building_persistent_local_id",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "building_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_building_unit_persistent_local_id",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "building_unit_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_geometry",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_is_removed",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_oslo_status",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_status",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_building_unit_versions_version_timestamp",
                schema: "integration_building",
                table: "building_unit_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_building_persistent_local_id",
                schema: "integration_building",
                table: "building_versions",
                column: "building_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_geometry",
                schema: "integration_building",
                table: "building_versions",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_is_removed",
                schema: "integration_building",
                table: "building_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_oslo_status",
                schema: "integration_building",
                table: "building_versions",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_status",
                schema: "integration_building",
                table: "building_versions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_building_versions_version_timestamp",
                schema: "integration_building",
                table: "building_versions",
                column: "version_timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "building_latest_items",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_address_versions",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_addresses",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_latest_items",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_readdress_versions",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_unit_versions",
                schema: "integration_building");

            migrationBuilder.DropTable(
                name: "building_versions",
                schema: "integration_building");
        }
    }
}
