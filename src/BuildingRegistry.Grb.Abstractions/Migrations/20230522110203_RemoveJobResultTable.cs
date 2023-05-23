using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Grb.Abstractions.Migrations
{
    public partial class RemoveJobResultTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobResults",
                schema: "BuildingRegistryGrb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobResults",
                schema: "BuildingRegistryGrb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    GrbIdn = table.Column<int>(type: "int", nullable: false),
                    IsBuildingCreated = table.Column<bool>(type: "bit", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobResults", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });
        }
    }
}
