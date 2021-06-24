using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddNullableUniqueConstraint1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId_1",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitDetails_PersistentLocalId_1",
                schema: "BuildingRegistryLegacy",
                table: "BuildingUnitDetails");
        }
    }
}
