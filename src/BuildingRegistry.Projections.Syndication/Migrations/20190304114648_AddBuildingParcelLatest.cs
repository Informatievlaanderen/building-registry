using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Syndication.Migrations
{
    public partial class AddBuildingParcelLatest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "BuildingParcelLatestItems",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(nullable: false),
                    CaPaKey = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingParcelLatestItems", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingParcelLatestItems",
                schema: "BuildingRegistryLegacy");
        }
    }
}
