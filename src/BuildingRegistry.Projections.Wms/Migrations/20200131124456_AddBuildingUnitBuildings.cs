using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    public partial class AddBuildingUnitBuildings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingPersistentLocalIds",
                schema: "wms");

            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "wms",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(nullable: true),
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
                name: "IX_BuildingUnit_Buildings_BuildingPersistentLocalId",
                schema: "wms",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "wms");

            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingPersistentLocalIds",
                schema: "wms",
                columns: table => new
                {
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingPersistentLocalId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingPersistentLocalIds", x => x.BuildingId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnit_BuildingPersistentLocalIds_BuildingPersistentLocalId",
                schema: "wms",
                table: "BuildingUnit_BuildingPersistentLocalIds",
                column: "BuildingPersistentLocalId");
        }
    }
}
