namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using NetTopologySuite.IO;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithValidPolygon());
        }

        [Fact]
        public void WithGeometryWhichShouldBeValid()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithGeometry(new WkbGeometry("01030000000100000017000000008813AE6C11074180E1962945510541007464FB7511074180779A6639510541001769F86B11074180546DD13151054100836E4577110741005CA9012251054100BD767F811107410050A7D62A5105410019B4A69E1107418050BBF905510541009330224612074100DB8DD886510541002E69DB1512074180C2E20AC7510541001985731212074100281692CB510541004C0EEE131207410068F3D2CC510541008327430A120741809B9933DA510541003E6BCD0B12074100DB7674DB51054100E79CEE0112074100759DE0EA510541009B68A630120741000528E02652054100616242F111074180173EF55752054100514CF44E110741809D82967D51054100D122F167110741808FE0E06A51054100A669667211074100A54F4579510541008679A591110741000CE98A6151054100FEC241C711074180E147CFAC51054100612535D9110741809AEF7A98510541008679A591110741000CE98A61510541008813AE6C11074180E1962945510541"));

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasMeasuredByGrb(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithGeometryMethodIsMeasuredByGrb()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasMeasuredByGrb(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithGeometryMethodIsOutlined()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasOutlined(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithGeometryMethodIsSurveyed()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasMeasuredByGrb(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenBuildingWasMeasuredWithOtherGeometryWithGeometryMethodIsSurveyed()
        {
            var validPolygon =
                "POLYGON ((141298.83027724177 185196.03552261365, 141294.79827723652 185190.20384261012, 141296.80672523379 185188.7793306075, 141295.2384692356 185186.52896260843, 141296.27578123659 185185.72653060779, 141294.88224523515 185183.81600260362, 141296.85165324062 185182.33645060286, 141298.27155724168 185184.30649860576, 141298.47520523518 185184.18451460451, 141304.05254924297 185192.11923461035, 141298.83027724177 185196.03552261365))";

            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(new WKTReader { DefaultSRID = WkbGeometry.SridLambert72 }.Read(validPolygon).AsBinary()));

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasMeasuredByGrb(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenBuildingWasMeasuredWithInvalidGeometry()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(null);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingGeometryWasRemoved(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenGeometryWasMeasuredUsingSameWkbWithGeometryMethodIsOutlined()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined);

            var buildingId = _fixture.Create<BuildingId>();
            var buildingWasMeasured = new BuildingWasMeasuredByGrb(buildingId, GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry));
            ((ISetProvenance)buildingWasMeasured).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingWasMeasured)
                .When(importStatus)
                .Then(buildingId,
                    new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithModificationDelete()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingGeometryWasRemoved(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithGeometryMethodAndGeometryIsSame()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            var buildingId = _fixture.Create<BuildingId>();
            var buildingWasMeasured = new BuildingWasMeasuredByGrb(buildingId, GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry));
            ((ISetProvenance)buildingWasMeasured).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingWasMeasured)
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoGeometryWithModificationDelete()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenMeasuredWithGeometryMethodIsOutlinedAndNewerLifetime()
        {
            var importedStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey);

            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithLifetime(new CrabLifetime(importedStatus.Lifetime.BeginDateTime.Value.PlusDays(1), null));

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>(),
                    importedStatus.ToLegacyEvent())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasOutlined(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenMeasuredWithGeometryMethodIsOutlinedAndOlderLifetime()
        {
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasuredByGrb>()
                .WithGeometry(_fixture.Create<WkbGeometry>());
            var importedGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(buildingWasMeasured.ExtendedWkb));

            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithLifetime(new CrabLifetime(importedGeometry.Lifetime.BeginDateTime.Value.PlusDays(-1), null));

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingWasMeasured,
                    importedGeometry.ToLegacyEvent())
                .When(importGeometry)
                .Then(_fixture.Create<BuildingId>(),
                    importGeometry.ToLegacyEvent()));
        }

        [Fact]
        public void WhenMeasuredAndRemovedWithGeometryMethodIsOutlinedAndSameLifetime()
        {
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasuredByGrb>();
            var importedGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(buildingWasMeasured.ExtendedWkb));

            var importedGeometryDelete = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(buildingWasMeasured.ExtendedWkb))
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(importedGeometry.Lifetime);

            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithLifetime(importedGeometry.Lifetime);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingWasMeasured,
                    importedGeometry.ToLegacyEvent(),
                    _fixture.Create<BuildingGeometryWasRemoved>(),
                    importedGeometryDelete.ToLegacyEvent())
                .When(importGeometry)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasOutlined(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importGeometry.BuildingGeometry)),
                    importGeometry.ToLegacyEvent()));
        }

        [Fact]
        public void WhenMeasuredAndOutlinedWithDeleteOfOlderGeometry()
        {
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasuredByGrb>()
                .WithGeometry(_fixture.Create<WkbGeometry>());
            var importedGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(buildingWasMeasured.ExtendedWkb));

            var importedGeometryDelete = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(importedGeometry.BuildingGeometryId)
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(buildingWasMeasured.ExtendedWkb))
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(importedGeometry.Lifetime);

            var buildingWasOutlined = _fixture.Create<BuildingWasOutlined>()
                .WithGeometry(_fixture.Create<WkbGeometry>());
            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithLifetime(importedGeometry.Lifetime)
                .WithGeometry(new WkbGeometry(buildingWasOutlined.ExtendedWkb));

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingWasMeasured,
                    importedGeometry.ToLegacyEvent(),
                    buildingWasOutlined,
                    importGeometry.ToLegacyEvent())
                .When(importedGeometryDelete)
                .Then(_fixture.Create<BuildingId>(),
                    importedGeometryDelete.ToLegacyEvent()));
        }
    }
}
