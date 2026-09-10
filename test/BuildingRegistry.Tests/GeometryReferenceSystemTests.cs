namespace BuildingRegistry.Tests
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Xunit;

    /// <summary>
    /// The one place that decides how a geometry moves between Lambert 72 and Lambert 2008, so that the
    /// migrator and the write side cannot disagree about it. See ADR 0007.
    /// </summary>
    public class GeometryReferenceSystemTests
    {
        private static Geometry Lambert72Polygon => GeometryHelper.ValidPolygon;

        /// <summary>
        /// What a plain <c>WKBReader</c> on the default geometry services hands back for the SRID-less bytes
        /// written before the event store wrote EWKB - which is what <c>BuildingGeometry</c> reads with.
        /// </summary>
        private static Geometry SridlessPolygon =>
            new WKBReader { HandleSRID = true }.Read(
                new WKBWriter { Strict = false, HandleSRID = false }.Write(GeometryHelper.ValidPolygon));

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, true)]
        [InlineData(SystemReferenceId.SridLambert2008, true)]
        [InlineData(0, false)]
        [InlineData(4326, false)]
        public void IsSupportedAnswersForTheLabelAlone(int srid, bool expected)
            => GeometryReferenceSystem.IsSupported(srid).Should().Be(expected);

        [Fact]
        public void ALabelledGeometryIsInTheSystemItsSridNames()
        {
            Lambert72Polygon.ReferenceSystem().Should().Be(SystemReferenceId.SridLambert72);

            Lambert72Polygon.ToReferenceSystem(SystemReferenceId.SridLambert2008)
                .ReferenceSystem().Should().Be(SystemReferenceId.SridLambert2008);
        }

        /// <summary>
        /// Geometries written before the event store wrote EWKB carry no SRID at all. They are Lambert 72 by
        /// definition, and reading them as an error rather than as Lambert 72 would reject exactly the rows
        /// the transformation exists for.
        /// </summary>
        [Fact]
        public void AnAbsentSridIsLambert72()
        {
            SridlessPolygon.SRID.Should().BeLessOrEqualTo(0);

            SridlessPolygon.ReferenceSystem().Should().Be(SystemReferenceId.SridLambert72);
        }

        [Fact]
        public void AGeometryWithoutASridIsTransformedAsLambert72()
        {
            var fromSridless = SridlessPolygon.ToReferenceSystem(SystemReferenceId.SridLambert2008);
            var fromLabelled = Lambert72Polygon.ToReferenceSystem(SystemReferenceId.SridLambert2008);

            fromSridless.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            ExtendedWkbGeometry.Create(fromSridless).Should().Be(ExtendedWkbGeometry.Create(fromLabelled));
        }

        /// <summary>Transforming an unlabelled geometry also puts the missing label on.</summary>
        [Fact]
        public void AGeometryWithoutASridIsLabelledEvenWhenItIsAlreadyInThatSystem()
            => SridlessPolygon.ToReferenceSystem(SystemReferenceId.SridLambert72).SRID
                .Should().Be(SystemReferenceId.SridLambert72);

        [Fact]
        public void AGeometryAlreadyInTheSystemIsHandedBackUntouched()
        {
            // The same instance, not merely an equal one: nothing to transform, nothing to relabel.
            var geometry = Lambert72Polygon;

            geometry.ToReferenceSystem(SystemReferenceId.SridLambert72).Should().BeSameAs(geometry);
        }

        [Fact]
        public void TheTransformIsReversible()
        {
            var roundTripped = Lambert72Polygon
                .ToReferenceSystem(SystemReferenceId.SridLambert2008)
                .ToReferenceSystem(SystemReferenceId.SridLambert72);

            roundTripped.SRID.Should().Be(SystemReferenceId.SridLambert72);

            for (var i = 0; i < Lambert72Polygon.Coordinates.Length; i++)
            {
                roundTripped.Coordinates[i].X.Should().BeApproximately(Lambert72Polygon.Coordinates[i].X, 0.001);
                roundTripped.Coordinates[i].Y.Should().BeApproximately(Lambert72Polygon.Coordinates[i].Y, 0.001);
            }
        }

        /// <summary>
        /// The rounding overload exists for positions, which are persisted at centimetre precision. It
        /// rounds whether or not the geometry had to move, so the result does not depend on which system it
        /// started in.
        /// </summary>
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72)]
        [InlineData(SystemReferenceId.SridLambert2008)]
        public void RoundingAppliesWhetherOrNotTheGeometryMoves(int srid)
        {
            var rounded = Lambert72Polygon.ToReferenceSystem(srid, GeometryReferenceSystem.PositionRoundingPrecision);

            rounded.Coordinates.Should().AllSatisfy(coordinate =>
            {
                coordinate.X.Should().Be(Math.Round(coordinate.X, GeometryReferenceSystem.PositionRoundingPrecision));
                coordinate.Y.Should().Be(Math.Round(coordinate.Y, GeometryReferenceSystem.PositionRoundingPrecision));
            });
        }

        /// <summary>RoundCoordinates mutates in place, so the caller's geometry must never be the one it gets.</summary>
        [Fact]
        public void RoundingDoesNotMutateTheCallersGeometry()
        {
            var geometry = GeometryHelper.ValidPolygon;
            var before = geometry.Coordinates[0].X;

            geometry.ToReferenceSystem(SystemReferenceId.SridLambert72, GeometryReferenceSystem.PositionRoundingPrecision);

            geometry.Coordinates[0].X.Should().Be(before);
        }

        [Fact]
        public void AnUnsupportedTargetThrows()
        {
            var act = () => Lambert72Polygon.ToReferenceSystem(4326);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AGeometryInAnUnsupportedSystemThrows()
        {
            var wgs84 = GeometryHelper.ValidPolygon.Copy();
            wgs84.SRID = 4326;

            var act = () => wgs84.ToReferenceSystem(SystemReferenceId.SridLambert2008);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
