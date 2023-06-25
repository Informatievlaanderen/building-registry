namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Building;
    using Building.Events;
    using Building.Exceptions;
    using Fixtures;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public sealed class MergeBuildings : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("6F6D3E1D-5F1B-4B0B-9F6C-9F4F5F6E6D6C");

        public BuildingPersistentLocalId NewBuildingPersistentLocalId { get; set; }

        public ExtendedWkbGeometry NewBuildingExtendedWkbGeometry { get; set; }

        public IEnumerable<BuildingPersistentLocalId> BuildingPersistentLocalIdsToMerge { get; set; }

        public Provenance Provenance { get; }

        public MergeBuildings(
            BuildingPersistentLocalId newBuildingPersistentLocalId,
            ExtendedWkbGeometry newBuildingExtendedWkbGeometry,
            IEnumerable<BuildingPersistentLocalId> buildingPersistentLocalIdsToMerge,
            Provenance provenance)
        {
            NewBuildingPersistentLocalId = newBuildingPersistentLocalId;
            NewBuildingExtendedWkbGeometry = newBuildingExtendedWkbGeometry;
            BuildingPersistentLocalIdsToMerge = buildingPersistentLocalIdsToMerge;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeBuildingMeasurement-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return NewBuildingPersistentLocalId;
            yield return NewBuildingExtendedWkbGeometry.ToString();

            foreach (var field in BuildingPersistentLocalIdsToMerge)
            {
                yield return field;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }

    public sealed class GivenBuildingToMergeDoesNotExists : BuildingRegistryTest
    {
        public GivenBuildingToMergeDoesNotExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenAggregateNotFoundExceptionWasThrown()
        {
            var command = Fixture.Create<MergeBuildings>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new AggregateNotFoundException(command.BuildingPersistentLocalIdsToMerge.First(),
                    typeof(Building))));
        }
    }

    public sealed class GivenNewBuildingAlreadyExists : BuildingRegistryTest
    {
        public GivenNewBuildingAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void ThenAggregateSourceExceptionIsThrown()
        {
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            var command = Fixture.Create<MergeBuildings>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.NewBuildingPersistentLocalId),
                    buildingWasPlanned)
                .When(command)
                .Throws(new AggregateSourceException(
                    $"Building with id {command.NewBuildingPersistentLocalId} already exists")));
        }
    }

    public sealed class GivenBuildingsToMergeExists : BuildingRegistryTest
    {
        public GivenBuildingsToMergeExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void WithOnlyOneBuilding()
        {
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                new List<BuildingPersistentLocalId> { new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId) },
                Fixture.Create<Provenance>()
            );

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.NewBuildingPersistentLocalId),
                    buildingWasPlanned)
                .When(command)
                .Throws(new BuildingMergerNeedsMoreThanOneBuildingException()));
        }

        [Fact]
        public void WithMoreThanTwentyBuildings()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(21).ToList(); //Need Unique Id per building

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x => new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            Assert(new Scenario()
                .Given(givenPlannedFacts.ToArray())
                .When(command)
                .Throws(new BuildingMergerHasTooManyBuildingsException()));
        }

        [Fact]
        public void WithNoBuildingUnits()
        {
            var rnd = new Random();

            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(rnd.Next(2, 20)).ToList(); //Need Unique Id per building

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x => new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var expectedPlannedFacts = buildingWasPlannedEvents
                .Select(x => new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                    new BuildingWasMerged(new BuildingPersistentLocalId(x.BuildingPersistentLocalId), new BuildingPersistentLocalId(command.NewBuildingPersistentLocalId))))
                .ToArray();

            var expectedFacts = new Fact[expectedPlannedFacts.Length + 1];
            expectedFacts[0] = new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId), command.ToBuildingMergerWasRealizedEvent());
            expectedPlannedFacts.CopyTo(expectedFacts, 1);

            Assert(new Scenario()
                .Given(givenPlannedFacts.ToArray())
                .When(command)
                .Then(expectedFacts));
        }
    }

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("Het gebouw werd samengevoegd.")]
    public sealed class BuildingWasMerged : IBuildingEvent
    {
        public const string EventName = "BuildingWasMerged"; // BE CAREFUL CHANGING THIS!!

        public int BuildingPersistentLocalId { get; }
        public int NewBuildingPersistentLocalId { get; }

        public ProvenanceData Provenance { get; private set; }

        public BuildingWasMerged(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalId newBuildingPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            NewBuildingPersistentLocalId = newBuildingPersistentLocalId;
        }

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(NewBuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }

    public sealed class BuildingMergerHasTooManyBuildingsException : BuildingRegistryException
    {
    }

    public sealed class BuildingMergerNeedsMoreThanOneBuildingException : BuildingRegistryException
    {
    }

    public static class MergeBuildingsExtensions
    {
        public static BuildingMergerWasRealized ToBuildingMergerWasRealizedEvent(this MergeBuildings command)
        {
            return new BuildingMergerWasRealized(
                command.NewBuildingPersistentLocalId,
                command.NewBuildingExtendedWkbGeometry,
                command.BuildingPersistentLocalIdsToMerge);
        }
    }

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("Gebouw samenvoeging werd gerealiseerd.")]
    public sealed class BuildingMergerWasRealized : IBuildingEvent
    {
        public const string EventName = "BuildingMergerWasRealized"; // BE CAREFUL CHANGING THIS!!

        public int BuildingPersistentLocalId { get; }
        public string ExtendedWkbGeometry { get; }
        public IEnumerable<int> MergedBuildingPersistentLocalIds { get; }

        public ProvenanceData Provenance { get; private set; }

        public BuildingMergerWasRealized(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry,
            IEnumerable<BuildingPersistentLocalId> mergedBuildingPersistentLocalIds)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            ExtendedWkbGeometry = extendedWkbGeometry;
            MergedBuildingPersistentLocalIds = mergedBuildingPersistentLocalIds.Select(x => (int)x);
        }

        [JsonConstructor]
        private BuildingMergerWasRealized(
            int buildingPersistentLocalId,
            string buildingGeometry,
            IEnumerable<int> mergedBuildingPersistentLocalIds,
            ProvenanceData provenance)
            : this(new BuildingPersistentLocalId(buildingPersistentLocalId),
                new ExtendedWkbGeometry(buildingGeometry),
                mergedBuildingPersistentLocalIds.Select(x => new BuildingPersistentLocalId(x)))
        {
            ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
        }

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(ExtendedWkbGeometry);

            fields.AddRange(MergedBuildingPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
