using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Grb.Abstractions.Migrations
{
    public partial class ReplaceJobRecordTicketUrlByTicketId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketUrl",
                schema: "BuildingRegistryGrb",
                table: "JobRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "TicketId",
                schema: "BuildingRegistryGrb",
                table: "JobRecords",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketId",
                schema: "BuildingRegistryGrb",
                table: "JobRecords");

            migrationBuilder.AddColumn<string>(
                name: "TicketUrl",
                schema: "BuildingRegistryGrb",
                table: "JobRecords",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
