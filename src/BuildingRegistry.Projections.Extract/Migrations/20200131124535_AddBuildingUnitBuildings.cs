using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Extract.Migrations
{
    public partial class AddBuildingUnitBuildings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryExtract",
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
                schema: "BuildingRegistryExtract",
                table: "BuildingUnit_Buildings",
                column: "BuildingPersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_Buildings",
                schema: "BuildingRegistryExtract");
        }
    }
}
