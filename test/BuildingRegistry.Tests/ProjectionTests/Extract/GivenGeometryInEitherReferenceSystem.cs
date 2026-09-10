namespace BuildingRegistry.Tests.ProjectionTests.Extract
{
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using Projections.Extract.BuildingExtract;
    using Projections.Extract.BuildingUnitExtract;
    using Wfs;
    using Xunit;

    /// <summary>
    /// The extract is published as Lambert 72 shapefiles — <c>Api.Extract</c> writes a <c>.prj</c> of
    /// <c>Belge_Lambert_1972</c> — and a shape record carries no SRID of its own, so nothing downstream
    /// could tell that a record held Lambert 2008 coordinates instead. The projection is therefore what
    /// keeps the <c>.prj</c> honest, whichever system the event store holds. See ADR 0008.
    /// </summary>
    public class GivenBuildingGeometryInEitherReferenceSystem
        : BuildingExtractProjectionTest<BuildingExtractV2EsriProjections>
    {
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, BuildingGeometryInEitherReferenceSystem.Lambert72Polygon)]
        [InlineData(SystemReferenceId.SridLambert2008, BuildingGeometryInEitherReferenceSystem.Lambert2008Polygon)]
        public async Task ThenTheShapeRecordIsWrittenInLambert72(int eventSrid, string eventPolygon)
        {
            await Sut
                .Given(BuildingGeometryInEitherReferenceSystem.BuildingWasPlanned(eventPolygon, eventSrid))
                .Then(async ct =>
                {
                    var building = await ct.BuildingExtractV2Esri.FindAsync(
                        BuildingGeometryInEitherReferenceSystem.BuildingPersistentLocalId);

                    building.Should().NotBeNull();
                    building!.ShapeRecordContentLength.Should().BeGreaterThan(0);

                    // The same physical outline whichever system it arrived in. Approximately, not exactly:
                    // a transformed outline is written at the precision the transform produces rather than
                    // rounded, since a building geometry's vertices carry more decimals than a centimetre.
                    building.MinimumX.Should().BeApproximately(141294.8, 0.01);
                    building.MaximumY.Should().BeApproximately(185196.04, 0.01);
                });
        }

        protected override BuildingExtractV2EsriProjections CreateProjection()
            => new BuildingExtractV2EsriProjections(ExtractConfig, Encoding);
    }

    /// <summary>
    /// The same for building unit positions, which are rounded to centimetres on the transformed path —
    /// the precision positions are persisted at. See ADR 0008.
    /// </summary>
    public class GivenBuildingUnitPositionInEitherReferenceSystem
        : BuildingExtractProjectionTest<BuildingUnitExtractV2Projections>
    {
        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, BuildingUnitPositionInEitherReferenceSystem.Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, BuildingUnitPositionInEitherReferenceSystem.Lambert2008Point)]
        public async Task ThenTheShapeRecordIsWrittenInLambert72(int eventSrid, string eventPoint)
        {
            await Sut
                .Given(BuildingUnitPositionInEitherReferenceSystem.BuildingUnitWasPlanned(eventPoint, eventSrid))
                .Then(async ct =>
                {
                    var unit = await ct.BuildingUnitExtractV2.FindAsync(
                        BuildingUnitPositionInEitherReferenceSystem.BuildingUnitPersistentLocalId);

                    unit.Should().NotBeNull();
                    unit!.ShapeRecordContentLength.Should().BeGreaterThan(0);

                    unit.MinimumX.Should().BeApproximately(141299, 0.01);
                    unit.MinimumY.Should().BeApproximately(185188, 0.01);
                });
        }

        protected override BuildingUnitExtractV2Projections CreateProjection()
            => new BuildingUnitExtractV2Projections(ExtractConfig, Encoding);
    }
}
