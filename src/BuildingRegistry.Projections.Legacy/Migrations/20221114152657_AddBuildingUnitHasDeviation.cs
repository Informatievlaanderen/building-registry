using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddBuildingUnitHasDeviation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropIndex(
            //     name: "IX_BuildingDetailsV2_PersistentLocalId",
            //     schema: "BuildingRegistryLegacy",
            //     table: "BuildingDetailsV2");

            migrationBuilder.AddColumn<bool>(
                name: "HasDeviation",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndicationV2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDeviation",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDeviation",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitSyndicationV2");

            migrationBuilder.DropColumn(
                name: "HasDeviation",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetailsV2");

            // migrationBuilder.CreateIndex(
            //     name: "IX_BuildingDetailsV2_PersistentLocalId",
            //     schema: "BuildingRegistryLegacy",
            //     table: "BuildingDetailsV2",
            //     column: "PersistentLocalId");
        }
    }
}
