using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class ExpandRenameBuildingUnit_Buildings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingOsloIds",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingOsloId = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: true),
                    IsRemoved = table.Column<bool>(nullable: false),
                    BuildingRetiredStatus = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_Buildings", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_Buildings_BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_Buildings",
                column: "BuildingOsloId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingOsloIds",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingOsloId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingOsloIds", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingOsloIds_BuildingOsloId",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnit_BuildingOsloIds",
                column: "BuildingOsloId");
        }
    }
}
