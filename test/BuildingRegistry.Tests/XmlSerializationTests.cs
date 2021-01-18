namespace BuildingRegistry.Tests
{
    using System;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Events;
    using FluentAssertions;
    using NodaTime;
    using Projections.Legacy.BuildingSyndication;
    using ValueObjects;
    using Xunit;

    public class XmlSerializationTests
    {
        [Fact]
        public void GivenReaddressedEvent_WhenSerializingToXml_ThenReturnsCorrectOutput()
        {
            var buildingGuid = "00000000-0000-0000-0000-000000000000";
            var buildingUnitGuid = "00000000-0000-0000-0000-000000000001";
            var oldAddressGuid = "00000000-0000-0000-0000-000000000002";
            var newAddressGuid = "00000000-0000-0000-0000-000000000003";
            var date = new DateTime(2021, 01, 30);

            var @event = new BuildingUnitWasReaddressed(
                new BuildingId(Guid.Parse(buildingGuid)),
                new BuildingUnitId(Guid.Parse(buildingUnitGuid)),
                new AddressId(Guid.Parse(oldAddressGuid)),
                new AddressId(Guid.Parse(newAddressGuid)),
                new ReaddressingBeginDate(LocalDate.FromDateTime(date)));

            var dateTimeOffset = new DateTimeOffset(2021, 01, 01, 15, 5, 30, 0, TimeSpan.FromHours(1));

            ((ISetProvenance)@event).SetProvenance(new Provenance(Instant.FromDateTimeOffset(dateTimeOffset), Application.BuildingRegistry, Reason.ManagementCrab, new Operator("tester"), Modification.Update, Organisation.Aiv));

            var result = @event.ToXml("BuildingUnitWasReaddressed").ToString(SaveOptions.DisableFormatting);

            result.Should().Be("<BuildingUnitWasReaddressed><BuildingId>00000000-0000-0000-0000-000000000000</BuildingId><BuildingUnitId>00000000-0000-0000-0000-000000000001</BuildingUnitId><OldAddressId>00000000-0000-0000-0000-000000000002</OldAddressId><NewAddressId>00000000-0000-0000-0000-000000000003</NewAddressId><BeginDate>2021-01-30</BeginDate><Provenance><Timestamp>2021-01-01T14:05:30Z</Timestamp><Organisation>Aiv</Organisation><Reason>Bijhouding op CRAB</Reason></Provenance></BuildingUnitWasReaddressed>");
        }
    }
}
