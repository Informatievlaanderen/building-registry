using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Consumer.Read.Parcel.Migrations
{
    public partial class AddParcelGeometry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ExtendedWkbGeometry",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItems",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Geometry>(
                name: "Geometry",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItems",
                type: "sys.geometry",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedWkbGeometry",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItems");

            migrationBuilder.DropColumn(
                name: "Geometry",
                schema: "BuildingRegistryConsumerReadParcel",
                table: "ParcelItems");
        }
    }
}
