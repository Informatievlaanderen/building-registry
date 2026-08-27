namespace BuildingRegistry.Tests.ProjectionTests.Wfs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using FluentAssertions;
    using Tests.Legacy.Autofixture;
    using Projections.Wfs.BuildingV3;
    using Projections.Wfs.BuildingV4;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    /// <summary>The same physical building outline, in both reference systems.</summary>
    public static class BuildingGeometryInEitherReferenceSystem
    {
        public const string Lambert72Polygon =
            "POLYGON ((141298.83 185196.04, 141294.8 185190.2, 141296.81 185188.78, 141295.24 185186.53, 141296.28 185185.73, 141294.88 185183.82, 141296.85 185182.34, 141298.27 185184.31, 141298.48 185184.18, 141304.05 185192.12, 141298.83 185196.04))";

        public const string Lambert2008Polygon =
            "POLYGON ((641296.8 685195.4, 641292.77 685189.56, 641294.78 685188.14, 641293.21 685185.89, 641294.25 685185.09, 641292.85 685183.18, 641294.82 685181.7, 641296.24 685183.67, 641296.45 685183.54, 641302.02 685191.48, 641296.8 685195.4))";

        public const int BuildingPersistentLocalId = 1;

        public static Envelope<BuildingWasPlannedV2> BuildingWasPlanned(string wkt, int srid)
        {
            var fixture = new Fixture();
            fixture.Customize(new InfrastructureCustomization());

            var @event = new BuildingWasPlannedV2(
                new BuildingPersistentLocalId(BuildingPersistentLocalId),
                GeometryHelper.CreateEwkbFromWkt(wkt, srid));
            ((ISetProvenance)@event).SetProvenance(fixture.Create<Provenance>());

            return new Envelope<BuildingWasPlannedV2>(new Envelope(@event, new Dictionary<string, object>()));
        }
    }

    /// <summary>
    /// Version 3 stores Lambert 72 whatever the event store persists, so its table, spatial index and
    /// views stay single-SRID through the conversion. See ADR 0005.
    /// </summary>
    public class GivenBuildingGeometryInEitherReferenceSystemV3 : BuildingWfsProjectionTest<BuildingV3Projections>
    {
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, BuildingGeometryInEitherReferenceSystem.Lambert72Polygon)]
        [InlineData(SystemReferenceId.SridLambert2008, BuildingGeometryInEitherReferenceSystem.Lambert2008Polygon)]
        public async Task ThenTheGeometryIsStoredInLambert72(int eventSrid, string eventPolygon)
        {
            await Sut
                .Given(BuildingGeometryInEitherReferenceSystem.BuildingWasPlanned(eventPolygon, eventSrid))
                .Then(async ct =>
                {
                    var building = await ct.BuildingsV3.FindAsync(BuildingGeometryInEitherReferenceSystem.BuildingPersistentLocalId);

                    building.Should().NotBeNull();
                    var geometry = building!.Geometry!;

                    geometry.SRID.Should().Be(SystemReferenceId.SridLambert72);

                    // The same physical outline whichever system it arrived in. Approximately, not exactly:
                    // a transformed polygon is stored at the precision the transform produces rather than
                    // rounded, so an outline that came in as Lambert 2008 lands within the transform's
                    // accuracy of one that came in as Lambert 72.
                    geometry.Coordinates[0].X.Should().BeApproximately(141298.83, 0.01);
                    geometry.Coordinates[0].Y.Should().BeApproximately(185196.04, 0.01);
                });
        }

        protected override BuildingV3Projections CreateProjection() => new BuildingV3Projections();
    }

    /// <summary>
    /// Version 4 is the same table one reference system further along: Lambert 2008, whatever the event
    /// store persists. See ADR 0005.
    /// </summary>
    public class GivenBuildingGeometryInEitherReferenceSystemV4 : BuildingWfsProjectionTest<BuildingV4Projections>
    {
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, BuildingGeometryInEitherReferenceSystem.Lambert72Polygon)]
        [InlineData(SystemReferenceId.SridLambert2008, BuildingGeometryInEitherReferenceSystem.Lambert2008Polygon)]
        public async Task ThenTheGeometryIsStoredInLambert2008(int eventSrid, string eventPolygon)
        {
            await Sut
                .Given(BuildingGeometryInEitherReferenceSystem.BuildingWasPlanned(eventPolygon, eventSrid))
                .Then(async ct =>
                {
                    var building = await ct.BuildingsV4.FindAsync(BuildingGeometryInEitherReferenceSystem.BuildingPersistentLocalId);

                    building.Should().NotBeNull();
                    var geometry = building!.Geometry!;

                    geometry.SRID.Should().Be(SystemReferenceId.SridLambert2008);

                    geometry.Coordinates[0].X.Should().BeApproximately(641296.8, 0.01);
                    geometry.Coordinates[0].Y.Should().BeApproximately(685195.4, 0.01);
                });
        }

        protected override BuildingV4Projections CreateProjection() => new BuildingV4Projections();
    }
}
