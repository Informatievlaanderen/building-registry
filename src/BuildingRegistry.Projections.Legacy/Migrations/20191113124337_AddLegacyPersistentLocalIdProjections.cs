using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    public partial class AddLegacyPersistentLocalIdProjections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DuplicatedPersistentLocalIds",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    DuplicatePersistentLocalId = table.Column<int>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: false),
                    OriginalPersistentLocalId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicatedPersistentLocalIds", x => x.DuplicatePersistentLocalId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RemovedPersistentLocalIds",
                schema: "BuildingRegistryLegacy",
                columns: table => new
                {
                    PersistentLocalId = table.Column<string>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: false),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemovedPersistentLocalIds", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuplicatedPersistentLocalIds",
                schema: "BuildingRegistryLegacy");

            migrationBuilder.DropTable(
                name: "RemovedPersistentLocalIds",
                schema: "BuildingRegistryLegacy");
        }
    }
}
