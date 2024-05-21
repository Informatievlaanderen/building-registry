using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Api.BackOffice.Abstractions.Migrations
{
    /// <inheritdoc />
    public partial class AddCountOnParcelAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count",
                schema: "BuildingRegistryBackOffice",
                table: "BuildingUnitAddressRelation",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                schema: "BuildingRegistryBackOffice",
                table: "BuildingUnitAddressRelation");
        }
    }
}
