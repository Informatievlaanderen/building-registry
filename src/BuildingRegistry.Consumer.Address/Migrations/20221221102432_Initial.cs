using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Consumer.Address.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BuildingRegistryConsumerAddress");

            migrationBuilder.CreateTable(
                name: "Addresses",
                schema: "BuildingRegistryConsumerAddress",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                schema: "BuildingRegistryConsumerAddress",
                columns: table => new
                {
                    IdempotenceKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateProcessed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.IdempotenceKey)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressId",
                schema: "BuildingRegistryConsumerAddress",
                table: "Addresses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_IsRemoved",
                schema: "BuildingRegistryConsumerAddress",
                table: "Addresses",
                column: "IsRemoved");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses",
                schema: "BuildingRegistryConsumerAddress");

            migrationBuilder.DropTable(
                name: "ProcessedMessages",
                schema: "BuildingRegistryConsumerAddress");
        }
    }
}
