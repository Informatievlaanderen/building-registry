using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Consumer.Read.Parcel.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressItems",
                schema: "BuildingRegistryConsumerReadParcel");

            migrationBuilder.DropTable(
                name: "ParcelItems",
                schema: "BuildingRegistryConsumerReadParcel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "ParcelItems",
                schema: "BuildingRegistryConsumerReadParcel",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExtendedWkbGeometry = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Geometry = table.Column<Geometry>(type: "sys.geometry", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelItems", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressItems_ParcelId",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelAddressItems",
                column: "ParcelId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelItems_CaPaKey",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItems",
                column: "CaPaKey");
        }
    }
}
