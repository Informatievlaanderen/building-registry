using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryExtract");

            migrationBuilder.CreateTable(
                name: "Building",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    MinimumX = table.Column<double>(nullable: false),
                    MaximumX = table.Column<double>(nullable: false),
                    MinimumY = table.Column<double>(nullable: false),
                    MaximumY = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Building", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "BuildingUnit",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: true),
                    OsloId = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    MinimumX = table.Column<double>(nullable: false),
                    MaximumX = table.Column<double>(nullable: false),
                    MinimumY = table.Column<double>(nullable: false),
                    MaximumY = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit", x => x.BuildingUnitId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "BuildingRegistryExtract",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Building_OsloId",
                schema: "BuildingRegistryExtract",
                table: "Building",
                column: "OsloId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingId",
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit",
                column: "BuildingId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Building",
                schema: "BuildingRegistryExtract");

            migrationBuilder.DropTable(
                name: "BuildingUnit",
                schema: "BuildingRegistryExtract");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "BuildingRegistryExtract");
        }
    }
}
