﻿// <auto-generated />
using System;
using BuildingRegistry.Grb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

#nullable disable

namespace BuildingRegistry.Grb.Abstractions.Migrations
{
    [DbContext(typeof(BuildingGrbContext))]
    [Migration("20230502113213_ReplaceJobTicketUrlByTicketId")]
    partial class ReplaceJobTicketUrlByTicketId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BuildingRegistry.Grb.Abstractions.Job", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastChanged")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid?>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Status");

                    b.ToTable("Jobs", "BuildingRegistryGrb");
                });

            modelBuilder.Entity("BuildingRegistry.Grb.Abstractions.JobRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<int?>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("EndDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<Polygon>("Geometry")
                        .IsRequired()
                        .HasColumnType("sys.geometry");

                    b.Property<int>("GrId")
                        .HasColumnType("int");

                    b.Property<int>("GrbObject")
                        .HasColumnType("int");

                    b.Property<int>("GrbObjectType")
                        .HasColumnType("int");

                    b.Property<long>("Idn")
                        .HasColumnType("bigint");

                    b.Property<int>("IdnVersion")
                        .HasColumnType("int");

                    b.Property<Guid>("JobId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal?>("Overlap")
                        .HasPrecision(8, 5)
                        .HasColumnType("decimal(8,5)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("TicketUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionDate")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.HasIndex("JobId");

                    b.ToTable("JobRecords", "BuildingRegistryGrb");
                });

            modelBuilder.Entity("BuildingRegistry.Grb.Abstractions.JobResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<int>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("GrbIdn")
                        .HasColumnType("int");

                    b.Property<bool>("IsBuildingCreated")
                        .HasColumnType("bit");

                    b.Property<Guid>("JobId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.ToTable("JobResults", "BuildingRegistryGrb");
                });
#pragma warning restore 612, 618
        }
    }
}
