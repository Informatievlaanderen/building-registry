namespace BuildingRegistry.Tests.AggregateTests.WhenReplacingAddressAttachmentFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BackOffice;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressReaddressed : BuildingRegistryTest
    {
        public GivenAddressReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        //TODO-jonas add missing State test for BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed
    }
}
