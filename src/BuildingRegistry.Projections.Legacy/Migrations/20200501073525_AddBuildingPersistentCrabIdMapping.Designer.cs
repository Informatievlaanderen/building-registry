﻿// <auto-generated />
using System;
using BuildingRegistry.Projections.Legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    [Migration("20200501073525_AddBuildingPersistentCrabIdMapping")]
    partial class AddBuildingPersistentCrabIdMapping
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingDetail.BuildingDetailItem", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Geometry")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("GeometryMethod")
                        .HasColumnType("int");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("Version")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("BuildingId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("Status");

                    b.HasIndex("IsComplete", "IsRemoved", "PersistentLocalId");

                    b.ToTable("BuildingDetails","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingPersistentIdCrabIdMapping.BuildingPersistentLocalIdCrabIdMapping", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CrabIdentifierTerrainObject")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CrabTerrainObjectId")
                        .HasColumnType("int");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("BuildingId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("CrabTerrainObjectId");

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("BuildingPersistentIdCrabIdMappings","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<int?>("Application")
                        .HasColumnType("int");

                    b.Property<Guid?>("BuildingId")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ChangeType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventDataAsXml")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Geometry")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("GeometryMethod")
                        .HasColumnType("int");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnName("LastChangedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Modification")
                        .HasColumnType("int");

                    b.Property<string>("Operator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Organisation")
                        .HasColumnType("int");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("RecordCreatedAtAsDateTimeOffset")
                        .HasColumnName("RecordCreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.HasKey("Position")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("BuildingId");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("Position")
                        .HasName("CI_BuildingSyndication_Position")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("BuildingSyndication","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitAddressSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<Guid>("BuildingUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.HasKey("Position", "BuildingUnitId", "AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("BuildingUnitAddressSyndication","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitReaddressSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<Guid>("BuildingUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OldAddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("NewAddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ReaddressBeginDateAsDateTimeOffset")
                        .HasColumnName("ReaddressDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Position", "BuildingUnitId", "OldAddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("BuildingUnitReaddressSyndication","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<Guid>("BuildingUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FunctionAsString")
                        .HasColumnName("Function")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("PointPosition")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PositionMethodAsString")
                        .HasColumnName("PositionMethod")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StatusAsString")
                        .HasColumnName("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("Version")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Position", "BuildingUnitId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("Position", "BuildingUnitId")
                        .HasName("CI_BuildingUnitSyndication_Position_BuildingUnitId")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("BuildingUnitSyndication","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetail.BuildingUnitBuildingItem", b =>
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

                    b.HasKey("BuildingId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("BuildingPersistentLocalId");

                    b.ToTable("BuildingUnit_Buildings","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetail.BuildingUnitDetailAddressItem", b =>
                {
                    b.Property<Guid>("BuildingUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.HasKey("BuildingUnitId", "AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("AddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("BuildingUnitAddresses","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetail.BuildingUnitDetailItem", b =>
                {
                    b.Property<Guid>("BuildingUnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("FunctionAsString")
                        .HasColumnName("Function")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsBuildingComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("Position")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PositionMethodAsString")
                        .HasColumnName("PositionMethod")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StatusAsString")
                        .HasColumnName("Status")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("Version")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("BuildingUnitId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("BuildingId");

                    b.HasIndex("BuildingPersistentLocalId");

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("StatusAsString");

                    b.HasIndex("IsComplete", "IsRemoved", "PersistentLocalId", "IsBuildingComplete", "BuildingPersistentLocalId");

                    b.ToTable("BuildingUnitDetails","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration.DuplicatedPersistentLocalId", b =>
                {
                    b.Property<int>("DuplicatePersistentLocalId")
                        .HasColumnType("int");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("OriginalPersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("DuplicatePersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("DuplicatedPersistentLocalIds","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration.RemovedPersistentLocalId", b =>
                {
                    b.Property<string>("PersistentLocalId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RemovedPersistentLocalIds","BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitAddressSyndicationItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitSyndicationItem", null)
                        .WithMany("Addresses")
                        .HasForeignKey("Position", "BuildingUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitReaddressSyndicationItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitSyndicationItem", null)
                        .WithMany("Readdresses")
                        .HasForeignKey("Position", "BuildingUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingUnitSyndicationItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndication.BuildingSyndicationItem", null)
                        .WithMany("BuildingUnits")
                        .HasForeignKey("Position")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetail.BuildingUnitDetailAddressItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingUnitDetail.BuildingUnitDetailItem", null)
                        .WithMany("Addresses")
                        .HasForeignKey("BuildingUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
