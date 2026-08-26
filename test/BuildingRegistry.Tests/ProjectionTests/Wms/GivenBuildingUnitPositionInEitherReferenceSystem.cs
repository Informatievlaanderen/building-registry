namespace BuildingRegistry.Tests.ProjectionTests.Wms
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
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Projections.Wms.BuildingUnitV2;
    using Projections.Wms.BuildingUnitV3;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    /// <summary>The same physical building unit position, in both reference systems.</summary>
    public static class BuildingUnitPositionInEitherReferenceSystem
    {
        public const string Lambert72Point = "POINT (141299 185188)";
        public const string Lambert2008Point = "POINT (641296.97 685187.36)";

        public const int BuildingUnitPersistentLocalId = 1;

        public static Envelope<BuildingUnitWasPlannedV2> BuildingUnitWasPlanned(string wkt, int srid)
        {
            var fixture = new Fixture();
            fixture.Customize(new InfrastructureCustomization());

            var @event = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(1),
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                GeometryHelper.CreateEwkbFromWkt(wkt, srid),
                BuildingUnitFunction.Unknown,
                false);
            ((ISetProvenance)@event).SetProvenance(fixture.Create<Provenance>());

            return new Envelope<BuildingUnitWasPlannedV2>(new Envelope(@event, new Dictionary<string, object>()));
        }

        /// <summary>
        /// WMS stores plain WKB, which carries no SRID: the reference system is the one the table's
        /// CalculatedGeometry computed column stamps on it. This reads the bytes back the way that column
        /// does, so the assertion is about what the geoserver ends up serving. See ADR 0005.
        /// </summary>
        public static Point ReadAsStoredByTheComputedColumn(byte[] wkb, int computedColumnSrid)
        {
            var position = (Point)new WKBReader().Read(wkb);
            position.SRID = computedColumnSrid;

            return position;
        }
    }

    /// <summary>
    /// Version 2 stores Lambert 72 whatever the event store persists, so its table, spatial index and
    /// views stay single-SRID through the conversion. See ADR 0005.
    /// </summary>
    public class GivenBuildingUnitPositionInEitherReferenceSystemV2 : BuildingWmsProjectionTest<BuildingUnitV2Projections>
    {
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, BuildingUnitPositionInEitherReferenceSystem.Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, BuildingUnitPositionInEitherReferenceSystem.Lambert2008Point)]
        public async Task ThenThePositionIsStoredInLambert72(int eventSrid, string eventPoint)
        {
            await Sut
                .Given(BuildingUnitPositionInEitherReferenceSystem.BuildingUnitWasPlanned(eventPoint, eventSrid))
                .Then(async ct =>
                {
                    var unit = await ct.BuildingUnitsV2.FindAsync(BuildingUnitPositionInEitherReferenceSystem.BuildingUnitPersistentLocalId);

                    unit.Should().NotBeNull();
                    var position = BuildingUnitPositionInEitherReferenceSystem.ReadAsStoredByTheComputedColumn(unit!.Position!, SystemReferenceId.SridLambert72);

                    position.SRID.Should().Be(SystemReferenceId.SridLambert72);

                    // Rounded back to the centimetre positions are persisted at, so a position that came
                    // in as Lambert 2008 is indistinguishable from one that came in as Lambert 72.
                    position.X.Should().Be(141299);
                    position.Y.Should().Be(185188);
                });
        }

        protected override BuildingUnitV2Projections CreateProjection() => new BuildingUnitV2Projections();
    }

    /// <summary>
    /// Version 3 is the same table one reference system further along: Lambert 2008, whatever the event
    /// store persists. See ADR 0005.
    /// </summary>
    public class GivenBuildingUnitPositionInEitherReferenceSystemV3 : BuildingWmsProjectionTest<BuildingUnitV3Projections>
    {
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, BuildingUnitPositionInEitherReferenceSystem.Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, BuildingUnitPositionInEitherReferenceSystem.Lambert2008Point)]
        public async Task ThenThePositionIsStoredInLambert2008(int eventSrid, string eventPoint)
        {
            await Sut
                .Given(BuildingUnitPositionInEitherReferenceSystem.BuildingUnitWasPlanned(eventPoint, eventSrid))
                .Then(async ct =>
                {
                    var unit = await ct.BuildingUnitsV3.FindAsync(BuildingUnitPositionInEitherReferenceSystem.BuildingUnitPersistentLocalId);

                    unit.Should().NotBeNull();
                    var position = BuildingUnitPositionInEitherReferenceSystem.ReadAsStoredByTheComputedColumn(unit!.Position!, SystemReferenceId.SridLambert2008);

                    position.SRID.Should().Be(SystemReferenceId.SridLambert2008);

                    position.X.Should().Be(641296.97);
                    position.Y.Should().Be(685187.36);
                });
        }

        protected override BuildingUnitV3Projections CreateProjection() => new BuildingUnitV3Projections();
    }
}
