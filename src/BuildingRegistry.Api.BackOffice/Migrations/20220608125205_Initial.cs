using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Api.BackOffice.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryBackOffice");

            migrationBuilder.CreateTable(
                name: "BuildingUnitBuildingRelationship",
                schema: "AddressRegistryBackOffice",
                columns: table => new
                {
                    BuildingUnitPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitBuildingRelationship", x => x.BuildingUnitPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitBuildingRelationship",
                schema: "AddressRegistryBackOffice");
        }
    }
}
