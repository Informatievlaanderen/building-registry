namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using BuildingRegistry.Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    /// <summary>
    /// Overlap validation reads the same <c>BuildingDetailsV2</c> spatial column matching does, and it fails
    /// the same silent way: SQL Server returns NULL rather than erroring on an SRID mismatch, so a check run
    /// against the wrong column finds no overlap at all. Here that does not merely lose a result — finding no
    /// overlap is exactly what lets an invalid building through. See ADR 0006.
    /// </summary>
    public class GivenOverlapCheckedInEitherReferenceSystem
    {
        private static readonly Lambert2008ConversionCompletedToggle Lambert72Checking = new(false);
        private static readonly Lambert2008ConversionCompletedToggle Lambert2008Checking = new(true);

        [Fact]
        public void WhenCheckingInLambert72_ThenAStoredBuildingStillOverlaps()
        {
            var context = ContextWith(Lambert72Checking, GeometryHelper.ValidPolygon);

            var result = context.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon))!);

            result.Should().ContainSingle();
        }

        /// <summary>
        /// The incoming outline arrives in Lambert 72 while the check is done in Lambert 2008 — the state the
        /// conversion window puts this in. It has to be brought across before anything is compared.
        /// </summary>
        [Fact]
        public void WhenCheckingInLambert2008_ThenALambert72OutlineStillOverlaps()
        {
            var context = ContextWith(Lambert2008Checking, GeometryHelper.ValidPolygon);

            var result = context.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon))!);

            result.Should().ContainSingle();
        }

        [Fact]
        public void WhenCheckingInLambert2008BeforeTheColumnIsPopulated_ThenItIsRefused()
        {
            var context = new FakeBuildingGeometryContextFactory(conversionCompleted: Lambert2008Checking)
                .CreateDbContext([]);

            // A row as it would be before the building event store's conversion reached it.
            context.BuildingGeometries.Add(new BuildingGeometryData(
                1,
                BuildingStatus.Realized,
                BuildingGeometryMethod.MeasuredByGrb,
                GeometryHelper.ValidPolygon,
                false));
            context.SaveChanges();

            var act = () => context.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon))!);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*buildings*no Lambert 2008 geometry*");
        }

        private static FakeBuildingGeometryContext ContextWith(
            Lambert2008ConversionCompletedToggle conversionCompleted,
            Geometry sysGeometry)
        {
            var context = new FakeBuildingGeometryContextFactory(conversionCompleted: conversionCompleted)
                .CreateDbContext([]);

            context.BuildingGeometries.Add(new BuildingGeometryData(
                1,
                BuildingStatus.Realized,
                BuildingGeometryMethod.MeasuredByGrb,
                sysGeometry,
                false,
                sysGeometry.EnsureLambert08()));
            context.SaveChanges();

            return context;
        }
    }
}
