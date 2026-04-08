using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Projections.Feed.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryFeed",
                table: "BuildingUnitDocuments",
                newName: "BuildingUnitPersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "BuildingRegistryFeed",
                table: "BuildingDocuments",
                newName: "BuildingPersistentLocalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BuildingUnitPersistentLocalId",
                schema: "BuildingRegistryFeed",
                table: "BuildingUnitDocuments",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "BuildingPersistentLocalId",
                schema: "BuildingRegistryFeed",
                table: "BuildingDocuments",
                newName: "PersistentLocalId");
        }
    }
}
