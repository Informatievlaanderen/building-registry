﻿// <auto-generated />
using System;
using BuildingRegistry.Projections.Wms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingRegistry.Projections.Wms.Migrations
{
    [DbContext(typeof(WmsContext))]
    partial class WmsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
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

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","wms");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Wms.Building.Building", b =>
                {
                    b.Property<Guid>("BuildingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Geometry")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("GeometryMethod")
                        .HasColumnType("varchar(12)")
                        .HasMaxLength(12);

                    b.Property<string>("Id")
                        .HasColumnType("varchar(46)")
                        .HasMaxLength(46);

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("StatusAsText")
                        .HasColumnName("Status")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("VersionAsString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("Version")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("BuildingId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("IsComplete");

                    b.HasIndex("StatusAsText");

                    b.ToTable("Buildings","wms");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Wms.BuildingUnit.BuildingUnit", b =>
                {
                    b.Property<Guid>("BuildingUnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("BuildingPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int?>("BuildingUnitPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("Function")
                        .HasColumnType("varchar(21)")
                        .HasMaxLength(21);

                    b.Property<string>("Id")
                        .HasColumnType("varchar(53)")
                        .HasMaxLength(53);

                    b.Property<bool>("IsBuildingComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Position")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PositionMethod")
                        .HasColumnType("varchar(22)")
                        .HasMaxLength(22);

                    b.Property<string>("StatusAsText")
                        .HasColumnName("Status")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("VersionAsString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("Version")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("BuildingUnitId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("BuildingId");

                    b.HasIndex("StatusAsText");

                    b.HasIndex("IsComplete", "IsBuildingComplete");

                    b.ToTable("BuildingUnits","wms");
                });

            modelBuilder.Entity("BuildingRegistry.Projections.Wms.BuildingUnit.BuildingUnitBuildingItem", b =>
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

                    b.ToTable("BuildingUnit_Buildings","wms");
                });
#pragma warning restore 612, 618
        }
    }
}
