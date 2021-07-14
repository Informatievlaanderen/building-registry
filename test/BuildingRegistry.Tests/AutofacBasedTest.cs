namespace BuildingRegistry.Tests
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.Comparers;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Infrastructure.Modules;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Building;
    using Building.Events;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public class TestConfig : IConfiguration
    {
        public IConfigurationSection GetSection(string key)
        {
            return null;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return new List<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken()
        {
            throw new System.NotImplementedException();
        }

        public string this[string key]
        {
            get => null;
            set => throw new System.NotImplementedException();
        }
    }

    public class AutofacBasedTest
    {
        protected IContainer Container { get; set; }

        protected readonly JsonSerializerSettings EventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        protected IExceptionCentricTestSpecificationRunner ExceptionCentricTestSpecificationRunner => Container.Resolve<IExceptionCentricTestSpecificationRunner>();

        protected IEventCentricTestSpecificationRunner EventCentricTestSpecificationRunner => Container.Resolve<IEventCentricTestSpecificationRunner>();

        protected IFactComparer FactComparer => Container.Resolve<IFactComparer>();

        protected IExceptionComparer ExceptionComparer => Container.Resolve<IExceptionComparer>();

        protected ILogger Logger => Container.Resolve<ILogger>();

        public AutofacBasedTest(ITestOutputHelper testOutputHelper)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .Build();

            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventSerializerSettings))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());

            var eventMappingDictionary =
                new Dictionary<string, System.Type>(
                    EventMapping.DiscoverEventNamesInAssembly(typeof(DomainAssemblyMarker).Assembly))
                {
                    {$"{nameof(SnapshotContainer)}<{nameof(BuildingSnapshot)}>", typeof(SnapshotContainer)}
                };

            containerBuilder.RegisterInstance(new EventMapping(eventMappingDictionary)).As<EventMapping>();

            containerBuilder
                .Register(c => new BuildingFactory(IntervalStrategy.SnapshotEvery(1000)))
                .As<IBuildingFactory>();

            containerBuilder.UseAggregateSourceTesting(CreateFactComparer(), CreateExceptionComparer());

            containerBuilder.RegisterInstance(testOutputHelper);
            containerBuilder.RegisterType<XUnitLogger>().AsImplementedInterfaces();
            containerBuilder.RegisterType<FakePersistentLocalIdGenerator>().As<IPersistentLocalIdGenerator>();

            Container = containerBuilder.Build();
        }

        protected virtual IFactComparer CreateFactComparer()
        {
            var comparer = new CompareLogic();
            comparer.Config.MembersToIgnore.Add("Provenance");
            return new CompareNetObjectsBasedFactComparer(comparer);
        }

        protected virtual IExceptionComparer CreateExceptionComparer()
        {
            var comparer = new CompareLogic();
            comparer.Config.MembersToIgnore.Add("Source");
            comparer.Config.MembersToIgnore.Add("StackTrace");
            comparer.Config.MembersToIgnore.Add("Message");
            comparer.Config.MembersToIgnore.Add("TargetSite");
            return new CompareNetObjectsBasedExceptionComparer(comparer);
        }

        protected void Assert(IExceptionCentricTestSpecificationBuilder builder)
            => builder.Assert(ExceptionCentricTestSpecificationRunner, ExceptionComparer, Logger);

        protected void Assert(IEventCentricTestSpecificationBuilder builder)
            => builder.Assert(EventCentricTestSpecificationRunner, FactComparer, Logger);

        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
