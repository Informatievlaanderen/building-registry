﻿// <auto-generated />
using System;
using BuildingRegistry.Consumer.Read.Parcel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Consumer.Read.Parcel.Migrations
{
    [DbContext(typeof(ConsumerParcelContext))]
    partial class ConsumerParcelContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name");

                    b.ToTable("ProjectionStates", "BuildingRegistryConsumerReadParcel");
                });

            modelBuilder.Entity("BuildingRegistry.Consumer.Read.Parcel.Parcel.ParcelAddressItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"), false);

                    b.HasIndex("ParcelId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("ParcelId"));

                    b.ToTable("ParcelAddressItems", "BuildingRegistryConsumerReadParcel");
                });

            modelBuilder.Entity("BuildingRegistry.Consumer.Read.Parcel.Parcel.ParcelConsumerItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("ExtendedWkbGeometry")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("sys.geometry");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"));

                    b.HasIndex("CaPaKey");

                    b.ToTable("ParcelItems", "BuildingRegistryConsumerReadParcel");
                });

            modelBuilder.Entity("BuildingRegistry.Consumer.Read.Parcel.ParcelWithCount.ParcelAddressItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"), false);

                    b.HasIndex("ParcelId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("ParcelId"));

                    b.ToTable("ParcelAddressItemsWithCount", "BuildingRegistryConsumerReadParcel");
                });

            modelBuilder.Entity("BuildingRegistry.Consumer.Read.Parcel.ParcelWithCount.ParcelConsumerItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("ExtendedWkbGeometry")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("sys.geometry");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"));

                    b.HasIndex("CaPaKey");

                    b.ToTable("ParcelItemsWithCount", "BuildingRegistryConsumerReadParcel");
                });
#pragma warning restore 612, 618
        }
    }
}
