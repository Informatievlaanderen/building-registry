﻿// <auto-generated />
using System;
using BuildingRegistry.Projections.Extract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BuildingRegistry.Projections.Extract.Migrations
{
    [DbContext(typeof(ExtractContext))]
    [Migration("20230425115045_RemoveBuildingUnitBuildingsV2")]
    partial class RemoveBuildingUnitBuildingsV2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

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

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Name"));

                    b.ToTable("ProjectionStates", "BuildingRegistryExtract");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Extract.BuildingExtract.BuildingExtractItem", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("ShapeRecordContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.HasKey("BuildingId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingId"), false);

                    b.HasIndex("PersistentLocalId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("PersistentLocalId"));

                    b.HasIndex("IsComplete", "ShapeRecordContentLength");

                    b.ToTable("Building", "BuildingRegistryExtract");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Extract.BuildingExtract.BuildingExtractItemV2", b =>
                {
                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.Property<byte[]>("ShapeRecordContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.HasKey("PersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("PersistentLocalId"));

                    b.HasIndex("MaximumX");

                    b.HasIndex("MaximumY");

                    b.HasIndex("MinimumX");

                    b.HasIndex("MinimumY");

                    b.HasIndex("ShapeRecordContentLength");

                    b.ToTable("BuildingV2", "BuildingRegistryExtract");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtract.BuildingUnitAddressLinkExtractItem", b =>
                {
                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("BuildingUnitPersistentLocalId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingUnitPersistentLocalId", "AddressPersistentLocalId"), false);

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("BuildingPersistentLocalId");

                    b.ToTable("BuildingUnitAddressLinks", "BuildingRegistryExtract");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Extract.BuildingUnitExtract.BuildingUnitBuildingItem", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int?>("BuildingRetiredStatus")
                        .HasColumnType("int");

                    b.Property<bool?>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.HasKey("BuildingId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingId"), false);

                    b.HasIndex("BuildingPersistentLocalId");

                    b.ToTable("BuildingUnit_Buildings", "BuildingRegistryExtract");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Extract.BuildingUnitExtract.BuildingUnitExtractItem", b =>
                {
                    b.Property<Guid>("BuildingUnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<bool>("IsBuildingComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("ShapeRecordContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.HasKey("BuildingUnitId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingUnitId"), false);

                    b.HasIndex("BuildingId");

                    b.HasIndex("PersistentLocalId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("PersistentLocalId"));

                    b.HasIndex("IsComplete", "IsBuildingComplete");

                    b.ToTable("BuildingUnit", "BuildingRegistryExtract");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Extract.BuildingUnitExtract.BuildingUnitExtractItemV2", b =>
                {
                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.Property<byte[]>("ShapeRecordContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.HasKey("BuildingUnitPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingUnitPersistentLocalId"));

                    b.HasIndex("BuildingPersistentLocalId");

                    b.ToTable("BuildingUnitV2", "BuildingRegistryExtract");
                });
#pragma warning restore 612, 618
        }
    }
}
