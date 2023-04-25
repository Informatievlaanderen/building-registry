using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveBuildingUnitBuildingsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "BuildingRegistryLegacy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnit_BuildingsV2",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingRetiredStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnit_BuildingsV2", x => x.BuildingPersistentLocalId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }
    }
}
