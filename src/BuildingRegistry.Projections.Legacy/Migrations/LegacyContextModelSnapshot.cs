﻿// <auto-generated />
using System;
using BuildingRegistry.Projections.Legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    partial class LegacyContextModelSnapshot : ModelSnapshot
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

                    b.ToTable("ProjectionStates", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingDetailV2.BuildingDetailItemV2", b =>
                {
                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("Geometry")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("GeometryMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<string>("LastEventHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StatusAsString")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Status");

                    b.Property<Geometry>("SysGeometry")
                        .HasColumnType("sys.geometry");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("Version");

                    b.HasKey("PersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("PersistentLocalId"));

                    b.HasIndex("IsRemoved");

                    b.HasIndex("StatusAsString");

                    b.HasIndex("IsRemoved", "StatusAsString");

                    b.ToTable("BuildingDetailsV2", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingDetailV2.BuildingDetailV2ListCountView", b =>
                {
                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.ToTable((string)null);

                    b.ToView("vw_BuildingDetailV2ListCountView", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingPersistentIdCrabIdMapping.BuildingPersistentLocalIdCrabIdMapping", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CrabIdentifierTerrainObject")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("CrabTerrainObjectId")
                        .HasColumnType("int");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("BuildingId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingId"), false);

                    b.HasIndex("CrabIdentifierTerrainObject");

                    b.HasIndex("CrabTerrainObjectId");

                    b.HasIndex("PersistentLocalId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("PersistentLocalId"));

                    b.ToTable("BuildingPersistentIdCrabIdMappings", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<int?>("Application")
                        .HasColumnType("int");

                    b.Property<Guid?>("BuildingId")
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
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("LastChangedOn");

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
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("RecordCreatedAt");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("SyndicationItemCreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Position");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position"));

                    b.HasIndex("BuildingId");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("Position")
                        .HasDatabaseName("CI_BuildingSyndicationWithCount_Position")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("BuildingSyndicationWithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitAddressSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<Guid>("BuildingUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.HasKey("Position", "BuildingUnitId", "AddressId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position", "BuildingUnitId", "AddressId"), false);

                    b.ToTable("BuildingUnitAddressSyndicationWithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitAddressSyndicationItemV2", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("Position", "BuildingUnitPersistentLocalId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position", "BuildingUnitPersistentLocalId", "AddressPersistentLocalId"), false);

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("BuildingUnitPersistentLocalId");

                    b.HasIndex("Position");

                    b.ToTable("BuildingUnitAddressSyndicationV2WithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitReaddressSyndicationItem", b =>
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
                        .HasColumnType("datetime2")
                        .HasColumnName("ReaddressDate");

                    b.HasKey("Position", "BuildingUnitId", "OldAddressId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position", "BuildingUnitId", "OldAddressId"), false);

                    b.ToTable("BuildingUnitReaddressSyndicationWithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<Guid>("BuildingUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FunctionAsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Function");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<byte[]>("PointPosition")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PositionMethodAsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("PositionMethod");

                    b.Property<string>("StatusAsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("Version");

                    b.HasKey("Position", "BuildingUnitId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position", "BuildingUnitId"), false);

                    b.HasIndex("Position", "BuildingUnitId")
                        .HasDatabaseName("CI_BuildingUnitSyndicationWithCount_Position_BuildingUnitId")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("BuildingUnitSyndicationWithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItemV2", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("Function")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("HasDeviation")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<byte[]>("PointPosition")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PositionMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("Version");

                    b.HasKey("Position", "PersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position", "PersistentLocalId"), false);

                    b.HasIndex("Position", "PersistentLocalId")
                        .HasDatabaseName("CI_BuildingUnitSyndicationV2WithCount_Position_BuildingUnitId")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("BuildingUnitSyndicationV2WithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2ListCountView", b =>
                {
                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.ToTable((string)null);

                    b.ToView("vw_BuildingUnitDetailV2ListCountView", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount.BuildingUnitDetailAddressItemV2", b =>
                {
                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("BuildingUnitPersistentLocalId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingUnitPersistentLocalId", "AddressPersistentLocalId"));

                    b.HasIndex("AddressPersistentLocalId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("AddressPersistentLocalId"), false);

                    b.HasIndex("BuildingUnitPersistentLocalId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("BuildingUnitPersistentLocalId"), false);

                    b.ToTable("BuildingUnitAddressesV2WithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount.BuildingUnitDetailItemV2", b =>
                {
                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("FunctionAsString")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Function");

                    b.Property<bool>("HasDeviation")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<string>("LastEventHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Position")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PositionMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StatusAsString")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("Version");

                    b.HasKey("BuildingUnitPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("BuildingUnitPersistentLocalId"));

                    b.HasIndex("BuildingPersistentLocalId");

                    b.HasIndex("FunctionAsString");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("StatusAsString");

                    b.ToTable("BuildingUnitDetailsV2WithCount", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration.DuplicatedPersistentLocalId", b =>
                {
                    b.Property<int>("DuplicatePersistentLocalId")
                        .HasColumnType("int");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("OriginalPersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("DuplicatePersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("DuplicatePersistentLocalId"), false);

                    b.ToTable("DuplicatedPersistentLocalIds", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration.RemovedPersistentLocalId", b =>
                {
                    b.Property<string>("PersistentLocalId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("PersistentLocalId"), false);

                    b.ToTable("RemovedPersistentLocalIds", "BuildingRegistryLegacy");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitAddressSyndicationItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItem", null)
                        .WithMany("Addresses")
                        .HasForeignKey("Position", "BuildingUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitAddressSyndicationItemV2", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItemV2", null)
                        .WithMany("Addresses")
                        .HasForeignKey("Position", "BuildingUnitPersistentLocalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitReaddressSyndicationItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItem", null)
                        .WithMany("Readdresses")
                        .HasForeignKey("Position", "BuildingUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItem", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingSyndicationItem", null)
                        .WithMany("BuildingUnits")
                        .HasForeignKey("Position")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItemV2", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingSyndicationItem", null)
                        .WithMany("BuildingUnitsV2")
                        .HasForeignKey("Position")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount.BuildingUnitDetailAddressItemV2", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount.BuildingUnitDetailItemV2", null)
                        .WithMany("Addresses")
                        .HasForeignKey("BuildingUnitPersistentLocalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingSyndicationItem", b =>
                {
                    b.Navigation("BuildingUnits");

                    b.Navigation("BuildingUnitsV2");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItem", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("Readdresses");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount.BuildingUnitSyndicationItemV2", b =>
                {
                    b.Navigation("Addresses");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount.BuildingUnitDetailItemV2", b =>
                {
                    b.Navigation("Addresses");
                });
#pragma warning restore 612, 618
        }
    }
}
