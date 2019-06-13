using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class BuildingUnitReaddressSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnitReaddressSyndication",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    OldAddressId = table.Column<Guid>(nullable: false),
                    NewAddressId = table.Column<Guid>(nullable: false),
                    ReaddressDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitReaddressSyndication", x => new { x.Position, x.BuildingUnitId, x.OldAddressId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_BuildingUnitReaddressSyndication_BuildingUnitSyndication_Position_BuildingUnitId",
                        columns: x => new { x.Position, x.BuildingUnitId },
                        principalSchema: "BuildingRegistryLegacy",
                        principalTable: "BuildingUnitSyndication",
                        principalColumns: new[] { "Position", "BuildingUnitId" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitReaddressSyndication",
                schema: "BuildingRegistryLegacy");
        }
    }
}
