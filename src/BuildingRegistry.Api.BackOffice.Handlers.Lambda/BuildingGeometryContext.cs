﻿namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class BuildingGeometryContext : DbContext, IBuildingGeometries
    {
        public DbSet<BuildingGeometryData> BuildingGeometries => Set<BuildingGeometryData>();

        public BuildingGeometryContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public BuildingGeometryContext(DbContextOptions<BuildingGeometryContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuildingGeometryData>()
                .ToTable("BuildingDetailsV2", Schema.Legacy)
                .HasKey(x => x.BuildingPersistentLocalId);

            modelBuilder.Entity<BuildingGeometryData>()
                .Property(x => x.GeometryMethod)
                .HasConversion(
                    x => x.Value,
                    x => BuildingGeometryMethod.Parse(x));
        }

        public ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var wkbReader = WKBReaderFactory.Create();
            var geometry = wkbReader.Read(extendedWkbGeometry);
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            var boundingBox = fixedGeometry.Factory.ToGeometry(fixedGeometry.EnvelopeInternal);

            var overlappingBuildings = BuildingGeometries
                .Where(building =>
                    building.BuildingPersistentLocalId != buildingPersistentLocalId
                    && boundingBox.Intersects(building.SysGeometry))
                .ToList()
                .Where(building => geometry.Intersects(building.SysGeometry))
                .ToList();

            return overlappingBuildings;
        }
    }
}
