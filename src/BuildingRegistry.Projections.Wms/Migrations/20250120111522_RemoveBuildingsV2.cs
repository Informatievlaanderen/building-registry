using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Wms.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBuildingsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingsV2",
                schema: "wms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingsV2",
                schema: "wms",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Geometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GeometryMethod = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    Id = table.Column<string>(type: "varchar(46)", maxLength: 46, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingsV2", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingsV2_Status",
                schema: "wms",
                table: "BuildingsV2",
                column: "Status");
        }
    }
}
