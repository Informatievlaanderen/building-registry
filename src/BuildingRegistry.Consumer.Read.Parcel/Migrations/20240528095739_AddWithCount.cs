using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Consumer.Read.Parcel.Migrations
{
    /// <inheritdoc />
    public partial class AddWithCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelAddressItemsWithCount",
                schema: "BuildingRegistryConsumerReadParcel",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddressItemsWithCount", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ParcelItemsWithCount",
                schema: "BuildingRegistryConsumerReadParcel",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtendedWkbGeometry = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelItemsWithCount", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressItemsWithCount_ParcelId",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelAddressItemsWithCount",
                column: "ParcelId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelItemsWithCount_CaPaKey",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItemsWithCount",
                column: "CaPaKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressItemsWithCount",
                schema: "BuildingRegistryConsumerReadParcel");

            migrationBuilder.DropTable(
                name: "ParcelItemsWithCount",
                schema: "BuildingRegistryConsumerReadParcel");
        }
    }
}
