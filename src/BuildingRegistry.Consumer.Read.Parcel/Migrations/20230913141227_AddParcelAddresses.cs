using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingRegistry.Consumer.Read.Parcel.Migrations
{
    public partial class AddParcelAddresses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelAddressItems",
                schema: "BuildingRegistryConsumerReadParcel",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddressItems", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressItems_ParcelId",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelAddressItems",
                column: "ParcelId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressItems",
                schema: "BuildingRegistryConsumerReadParcel");
        }
    }
}
