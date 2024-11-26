namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Building.Events;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.IO;
    using Producer;
    using Producer.Snapshot.Oslo;
    using Projections.BackOffice;
    using Projections.Extract;
    using Projections.Extract.BuildingExtract;
    using Projections.Extract.BuildingUnitAddressLinkExtractWithCount;
    using Projections.Extract.BuildingUnitExtract;
    using Projections.Integration;
    using Projections.Integration.Building.LatestItem;
    using Projections.Integration.Building.Version;
    using Projections.Integration.BuildingUnit.LatestItem;
    using Projections.Integration.Infrastructure;
    using Projections.LastChangedList;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetailV2;
    using Projections.Legacy.BuildingSyndicationWithCount;
    using Projections.Legacy.BuildingUnitDetailV2WithCount;
    using Projections.Wfs;
    using Projections.Wms;
    using Projections.Wms.BuildingUnitV2;
    using Projections.Wms.BuildingV2;
    using Xunit;

    public sealed class ProjectionsHandlesEventsTests
    {
        private readonly IEnumerable<Type> _eventsToExclude = [typeof(BuildingSnapshot)];
        private readonly IList<Type> _eventTypes;

        public ProjectionsHandlesEventsTests()
        {
            _eventTypes = DiscoverEventTypes();
        }

        private IList<Type> DiscoverEventTypes()
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => InfrastructureEventsTests.GetAssemblyTypesSafe(a)
                    .Any(t => t.Name == "DomainAssemblyMarker"));

            if (domainAssembly == null)
            {
                return Enumerable.Empty<Type>().ToList();
            }

            return domainAssembly.GetTypes()
                .Where(t => t is { IsClass: true, Namespace: not null }
                            && IsEventNamespace(t)
                            && IsNotCompilerGenerated(t)
                            && t.GetCustomAttributes(typeof(EventNameAttribute), true).Length != 0)
                .Except(_eventsToExclude)
                .ToList();
        }

        private static bool IsEventNamespace(Type t) => t.Namespace?.EndsWith("Building.Events") ?? false;
        private static bool IsNotCompilerGenerated(MemberInfo t) => Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null;

        [Fact]
        public void BackOfficeProjectionHandlesEvents()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "DelayInSeconds", "10" }
            };
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var projectionsToTest = new List<ConnectedProjection<BackOfficeProjectionsContext>>
            {
                new BackOfficeProjections(
                    Mock.Of<IDbContextFactory<BackOfficeContext>>(),
                    configurationRoot)
            };

            AssertHandleEvents(projectionsToTest, [
                typeof(BuildingBecameUnderConstructionV2),
                typeof(BuildingGeometryWasImportedFromGrb),
                typeof(BuildingMeasurementWasChanged),
                typeof(BuildingMeasurementWasCorrected),
                typeof(BuildingOutlineWasChanged),
                typeof(BuildingUnitDeregulationWasCorrected),
                typeof(BuildingUnitPositionWasCorrected),
                typeof(BuildingUnitRegularizationWasCorrected),
                typeof(BuildingUnitRemovalWasCorrected),
                typeof(BuildingUnitWasCorrectedFromNotRealizedToPlanned),
                typeof(BuildingUnitWasCorrectedFromRealizedToPlanned),
                typeof(BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected),
                typeof(BuildingUnitWasCorrectedFromRetiredToRealized),
                typeof(BuildingUnitWasDeregulated),
                typeof(BuildingUnitWasMovedIntoBuilding),
                typeof(BuildingUnitWasNotRealizedBecauseBuildingWasDemolished),
                typeof(BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized),
                typeof(BuildingUnitWasNotRealizedV2),
                typeof(BuildingUnitWasRealizedBecauseBuildingWasRealized),
                typeof(BuildingUnitWasRealizedV2),
                typeof(BuildingUnitWasRegularized),
                typeof(BuildingUnitWasRemovedV2),
                typeof(BuildingUnitWasRemovedBecauseBuildingWasRemoved),
                typeof(BuildingUnitWasRetiredBecauseBuildingWasDemolished),
                typeof(BuildingUnitWasRetiredV2),
                typeof(BuildingWasCorrectedFromNotRealizedToPlanned),
                typeof(BuildingWasCorrectedFromRealizedToUnderConstruction),
                typeof(BuildingWasCorrectedFromUnderConstructionToPlanned),
                typeof(BuildingWasDemolished),
                typeof(BuildingWasMeasured),
                typeof(BuildingWasNotRealizedV2),
                typeof(BuildingWasPlannedV2),
                typeof(BuildingWasRealizedV2),
                typeof(BuildingWasRemovedV2),
                typeof(UnplannedBuildingWasRealizedAndMeasured)
            ]);
        }

        [Theory]
        [MemberData(nameof(GetProjectionsToTest))]
        public void ProjectionsHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest)
        {
            AssertHandleEvents(projectionsToTest);
        }

        public static IEnumerable<object[]> GetProjectionsToTest()
        {
            yield return [new List<ConnectedProjection<LegacyContext>>
            {
                new BuildingDetailV2Projections(),
                new BuildingUnitDetailV2Projections(),
                new BuildingSyndicationProjections()
            }];

            yield return [new List<ConnectedProjection<WmsContext>>
            {
               new BuildingV2Projections(new WKBReader()),
               new BuildingUnitV2Projections(new WKBReader())
            }];

            yield return [new List<ConnectedProjection<WfsContext>>
            {
                new Projections.Wfs.BuildingV2.BuildingV2Projections(new WKBReader()),
                new Projections.Wfs.BuildingUnitV2.BuildingUnitV2Projections(new WKBReader())
            }];

            yield return [new List<ConnectedProjection<LastChangedListContext>>
            {
                new BuildingProjections(Mock.Of<ICacheValidator>()),
                new BuildingUnitProjections(Mock.Of<ICacheValidator>())
            }];

            yield return [new List<ConnectedProjection<IntegrationContext>>
            {
                new BuildingLatestItemProjections(Mock.Of<IOptions<IntegrationOptions>>()),
                new BuildingUnitLatestItemProjections(Mock.Of<IOptions<IntegrationOptions>>()),
                new BuildingVersionProjections(Mock.Of<IOptions<IntegrationOptions>>(), Mock.Of<IPersistentLocalIdFinder>(), Mock.Of<IAddresses>()),
                new BuildingRegistry.Projections.Integration.Building.VersionFromMigration.BuildingVersionProjections(Mock.Of<IOptions<IntegrationOptions>>())
            }];

            yield return [new List<ConnectedProjection<ExtractContext>>
            {
                new BuildingExtractV2EsriProjections(new OptionsWrapper<ExtractConfig>(new ExtractConfig()), Encoding.UTF8, new WKBReader()),
                new BuildingUnitExtractV2Projections(new OptionsWrapper<ExtractConfig>(new ExtractConfig()), Encoding.UTF8, new WKBReader()),
                new BuildingUnitAddressLinkExtractProjections(Encoding.UTF8),
            }];

            yield return [new List<ConnectedProjection<BuildingRegistry.Producer.Snapshot.Oslo.ProducerContext>>
            {
                new ProducerBuildingProjections(Mock.Of<IProducer>(), Mock.Of<ISnapshotManager>(), "", Mock.Of<IOsloProxy>()),
                new ProducerBuildingUnitProjections(Mock.Of<IProducer>(), Mock.Of<ISnapshotManager>(), "", Mock.Of<IOsloProxy>())
            }];

            yield return [new List<ConnectedProjection<BuildingRegistry.Producer.ProducerContext>>
            {
                new ProducerMigrateProjections(Mock.Of<IProducer>())
            }];
        }

        private void AssertHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest, IList<Type>? eventsToExclude = null)
        {
            var eventsToCheck = _eventTypes.Except(eventsToExclude ?? Enumerable.Empty<Type>()).ToList();
            foreach (var projection in projectionsToTest)
            {
                projection.Handlers.Should().NotBeEmpty();
                foreach (var eventType in eventsToCheck)
                {
                    var messageType = projection.Handlers.Any(x => x.Message.GetGenericArguments().First() == eventType);
                    messageType.Should().BeTrue($"The event {eventType.Name} is not handled by the projection {projection.GetType().Name}");
                }
            }
        }
    }
}
