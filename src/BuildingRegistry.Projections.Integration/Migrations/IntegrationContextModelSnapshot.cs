﻿// <auto-generated />
using System;
using BuildingRegistry.Projections.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BuildingRegistry.Projections.Integration.Migrations
{
    [DbContext(typeof(IntegrationContext))]
    partial class IntegrationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("DesiredState")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name");

                    b.ToTable("ProjectionStates", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.LatestItem.BuildingLatestItem", b =>
                {
                    b.Property<int>("BuildingPersistentLocalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("building_persistent_local_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("BuildingPersistentLocalId"));

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("GeometryMethod")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("geometry_method");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("boolean")
                        .HasColumnName("is_removed");

                    b.Property<string>("Namespace")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<string>("NisCode")
                        .HasColumnType("text")
                        .HasColumnName("nis_code");

                    b.Property<string>("OsloGeometryMethod")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_geometry_method");

                    b.Property<string>("OsloStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<string>("Puri")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("puri");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("BuildingPersistentLocalId");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("NisCode");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("Status");

                    b.HasIndex("IsRemoved", "Status");

                    b.ToTable("building_latest_items", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitAddressVersion", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint")
                        .HasColumnName("position");

                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_unit_persistent_local_id");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("address_persistent_local_id");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1)
                        .HasColumnName("count");

                    b.HasKey("Position", "BuildingUnitPersistentLocalId", "AddressPersistentLocalId");

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("BuildingUnitPersistentLocalId");

                    b.HasIndex("Position");

                    b.ToTable("building_unit_address_versions", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitReaddressVersion", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint")
                        .HasColumnName("position");

                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_unit_persistent_id");

                    b.Property<Guid>("OldAddressId")
                        .HasColumnType("uuid")
                        .HasColumnName("old_address_id");

                    b.Property<Guid>("NewAddressId")
                        .HasColumnType("uuid")
                        .HasColumnName("new_address_id");

                    b.Property<DateTime>("ReaddressBeginDateAsDateTimeOffset")
                        .HasColumnType("date")
                        .HasColumnName("readdress_date");

                    b.HasKey("Position", "BuildingUnitPersistentLocalId", "OldAddressId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("building_unit_readdress_versions", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitVersion", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint")
                        .HasColumnName("position");

                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_unit_persistent_local_id");

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_persistent_local_id");

                    b.Property<Guid?>("BuildingUnitId")
                        .HasColumnType("uuid")
                        .HasColumnName("building_unit_id");

                    b.Property<string>("CreatedOnAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_on_as_string");

                    b.Property<DateTimeOffset>("CreatedOnTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_timestamp");

                    b.Property<string>("Function")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("function");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("GeometryMethod")
                        .HasColumnType("text")
                        .HasColumnName("geometry_method");

                    b.Property<bool>("HasDeviation")
                        .HasColumnType("boolean")
                        .HasColumnName("has_deviation");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("boolean")
                        .HasColumnName("is_removed");

                    b.Property<string>("Namespace")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<string>("OsloFunction")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_function");

                    b.Property<string>("OsloGeometryMethod")
                        .HasColumnType("text")
                        .HasColumnName("oslo_geometry_method");

                    b.Property<string>("OsloStatus")
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<string>("PuriId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("puri_id");

                    b.Property<string>("Status")
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("Position", "BuildingUnitPersistentLocalId");

                    b.HasIndex("BuildingPersistentLocalId");

                    b.HasIndex("BuildingUnitPersistentLocalId");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("Status");

                    b.HasIndex("Type");

                    b.HasIndex("VersionTimestampAsDateTimeOffset");

                    b.ToTable("building_unit_versions", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingVersion", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint")
                        .HasColumnName("position");

                    b.Property<Guid?>("BuildingId")
                        .HasColumnType("uuid")
                        .HasColumnName("building_id");

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_persistent_local_id");

                    b.Property<string>("CreatedOnAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("created_on_as_string");

                    b.Property<DateTimeOffset>("CreatedOnTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on_timestamp");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("GeometryMethod")
                        .HasColumnType("text")
                        .HasColumnName("geometry_method");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("boolean")
                        .HasColumnName("is_removed");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_changed_on_timestamp");

                    b.Property<string>("LastChangedOnAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("last_changed_on_as_string");

                    b.Property<string>("Namespace")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<string>("OsloGeometryMethod")
                        .HasColumnType("text")
                        .HasColumnName("oslo_geometry_method");

                    b.Property<string>("OsloStatus")
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<string>("PuriId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("puri_id");

                    b.Property<string>("Status")
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("Position");

                    b.HasIndex("BuildingPersistentLocalId");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("Status");

                    b.HasIndex("Type");

                    b.HasIndex("VersionTimestampAsDateTimeOffset");

                    b.ToTable("building_versions", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.BuildingUnit.LatestItem.BuildingUnitAddress", b =>
                {
                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_unit_persistent_local_id");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("address_persistent_local_id");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.HasKey("BuildingUnitPersistentLocalId", "AddressPersistentLocalId");

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("BuildingUnitPersistentLocalId");

                    b.ToTable("building_unit_addresses", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.BuildingUnit.LatestItem.BuildingUnitLatestItem", b =>
                {
                    b.Property<int>("BuildingUnitPersistentLocalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("building_unit_persistent_local_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("BuildingUnitPersistentLocalId"));

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("building_persistent_local_id");

                    b.Property<string>("Function")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("function");

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("GeometryMethod")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("geometry_method");

                    b.Property<bool>("HasDeviation")
                        .HasColumnType("boolean")
                        .HasColumnName("has_deviation");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("boolean")
                        .HasColumnName("is_removed");

                    b.Property<string>("Namespace")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<string>("OsloFunction")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_function");

                    b.Property<string>("OsloGeometryMethod")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_geometry_method");

                    b.Property<string>("OsloStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<string>("Puri")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("puri");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("BuildingUnitPersistentLocalId");

                    b.HasIndex("BuildingPersistentLocalId");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("Status");

                    b.HasIndex("IsRemoved", "Status");

                    b.ToTable("building_unit_latest_items", "integration_building");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.MunicipalityGeometry", b =>
                {
                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<string>("NisCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("nis_code");

                    b.ToTable((string)null);

                    b.ToView("municipality_geometries", "integration_municipality");

                    b.ToSqlQuery("SELECT nis_code, geometry FROM integration_municipality.municipality_geometries");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitAddressVersion", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitVersion", null)
                        .WithMany("Addresses")
                        .HasForeignKey("Position", "BuildingUnitPersistentLocalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitReaddressVersion", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitVersion", null)
                        .WithMany("Readdresses")
                        .HasForeignKey("Position", "BuildingUnitPersistentLocalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitVersion", b =>
                {
                    b.HasOne("BuildingRegistry.Projections.Integration.Building.Version.BuildingVersion", null)
                        .WithMany("BuildingUnits")
                        .HasForeignKey("Position")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingUnitVersion", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("Readdresses");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Integration.Building.Version.BuildingVersion", b =>
                {
                    b.Navigation("BuildingUnits");
                });
#pragma warning restore 612, 618
        }
    }
}
