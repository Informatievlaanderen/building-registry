﻿// <auto-generated />
using System;
using BuildingRegistry.Projections.Wfs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace BuildingRegistry.Projections.Wfs.Migrations
{
    [DbContext(typeof(WfsContext))]
    [Migration("20200708135959_Initial_Building")]
    partial class Initial_Building
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
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

                    b.ToTable("ProjectionStates","wfs");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Wfs.Building.Building", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("sys.geometry");

                    b.Property<string>("GeometryMethod")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("VersionAsString")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("Version")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("BuildingId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("GeometryMethod");

                    b.HasIndex("Id");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("Status");

                    b.HasIndex("VersionAsString");

                    b.HasIndex("IsComplete", "IsRemoved");

                    b.ToTable("Buildings","wfs");
                });
#pragma warning restore 612, 618
        }
    }
}
