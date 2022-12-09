namespace BuildingRegistry.Tests.Legacy
{
    using System;
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.Comparers;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Building;
    using Infrastructure.Modules;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Moq;
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
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get => null;
            set => throw new NotImplementedException();
        }
    }

    public class AutofacBasedTest
    {
        private readonly IContainer _container;

        protected IExceptionCentricTestSpecificationRunner ExceptionCentricTestSpecificationRunner => _container.Resolve<IExceptionCentricTestSpecificationRunner>();

        protected IEventCentricTestSpecificationRunner EventCentricTestSpecificationRunner => _container.Resolve<IEventCentricTestSpecificationRunner>();

        protected IFactComparer FactComparer => _container.Resolve<IFactComparer>();

        protected IExceptionComparer ExceptionComparer => _container.Resolve<IExceptionComparer>();

        protected ILogger Logger => _container.Resolve<ILogger>();

        public AutofacBasedTest(ITestOutputHelper testOutputHelper)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Snapshots", "x" } })
                .Build();

            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());

            containerBuilder.RegisterModule(new SqlSnapshotStoreModule());

            containerBuilder.UseAggregateSourceTesting(CreateFactComparer(), CreateExceptionComparer());

            containerBuilder.RegisterInstance(testOutputHelper);
            containerBuilder.RegisterType<XUnitLogger>().AsImplementedInterfaces();
            containerBuilder.RegisterType<FakePersistentLocalIdGenerator>().As<IPersistentLocalIdGenerator>();

            containerBuilder.Register(c => new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>()))
                .As<BackOfficeContext>();

            containerBuilder.RegisterType<AddCommonBuildingUnit>()
                .As<IAddCommonBuildingUnit>();

            containerBuilder
                .Register(c => new BuildingFactory(NoSnapshotStrategy.Instance, c.Resolve<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()))
                .As<IBuildingFactory>();

            _container = containerBuilder.Build();
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
    }
}
