namespace BuildingRegistry.Tests.ProjectionTests.Integration.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Events;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using Projections.Integration.Converters;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingUnitFunction = BuildingRegistry.Building.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Building.BuildingUnitPositionGeometryMethod;
    using ReaddressingBeginDate = BuildingRegistry.Legacy.ReaddressingBeginDate;

    public partial class BuildingUnitVersionProjectionsTests
    {
        [Fact]
        public async Task WhenBuildingUnitPersistentLocalIdWasAssigned()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPersistentLocalIdWasAssigned = _fixture.Create<BuildingUnitPersistentLocalIdWasAssigned>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPersistentLocalIdWasAssignedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPersistentLocalIdWasAssigned>(new Envelope(buildingUnitPersistentLocalIdWasAssigned, buildingUnitPersistentLocalIdWasAssignedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalIdWasAssigned.PersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitPersistentLocalIdWasAssigned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitBecameComplete()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitBecameComplete = _fixture.Create<BuildingUnitBecameComplete>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitBecameCompleteMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitBecameComplete>(new Envelope(buildingUnitBecameComplete, buildingUnitBecameCompleteMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitBecameComplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitBecameIncomplete()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitBecameIncomplete = _fixture.Create<BuildingUnitBecameIncomplete>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitBecameIncompleteMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitBecameIncomplete>(new Envelope(buildingUnitBecameIncomplete, buildingUnitBecameIncompleteMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitBecameIncomplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasAdded()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingPersistentLocalId = (int)_fixture.Create<BuildingPersistentLocalId>();
            var addressPersistentLocalId = (int)_fixture.Create<AddressPersistentLocalId>();
            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();

            AddBuildingPersistentLocalId(buildingPersistentLocalId);
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId, addressPersistentLocalId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingId.Should().Be(buildingUnitWasAdded.BuildingId);
                    buildingUnitVersion.BuildingUnitId.Should().Be(buildingUnitWasAdded.BuildingUnitId);
                    buildingUnitVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalId);
                    buildingUnitVersion.BuildingUnitPersistentLocalId.Should().Be(buildingUnitPersistentLocalId);
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Unknown.Function);
                    buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Unknown.Map());
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(buildingUnitWasAdded.Provenance.Timestamp);
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasAdded.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitPersistentLocalId}");
                    buildingUnitVersion.Addresses.Should().ContainSingle();
                    buildingUnitVersion.Addresses.Single().AddressPersistentLocalId.Should().Be(addressPersistentLocalId);
                    buildingUnitVersion.Addresses.Single().Position.Should().Be(position);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasReaddedByOtherUnitRemoval()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingPersistentLocalId = (int) _fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = (int) _fixture.Create<AddressPersistentLocalId>();

            var buildingUnitWasReaddedByOtherUnitRemoval = _fixture.Create<BuildingUnitWasReaddedByOtherUnitRemoval>();

            AddBuildingPersistentLocalId(buildingPersistentLocalId);
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasReaddedByOtherUnitRemoval.AddressId, addressPersistentLocalId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>(new Envelope(buildingUnitWasReaddedByOtherUnitRemoval, buildingUnitWasAddedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingId.Should().Be(buildingUnitWasReaddedByOtherUnitRemoval.BuildingId);
                    buildingUnitVersion.BuildingUnitId.Should().Be(buildingUnitWasReaddedByOtherUnitRemoval.BuildingUnitId);
                    buildingUnitVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalId);
                    buildingUnitVersion.BuildingUnitPersistentLocalId.Should().Be(buildingUnitPersistentLocalId);
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Unknown.Function);
                    buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Unknown.Map());
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(buildingUnitWasReaddedByOtherUnitRemoval.Provenance.Timestamp);
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasReaddedByOtherUnitRemoval.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitPersistentLocalId}");
                    buildingUnitVersion.Addresses.Should().ContainSingle();
                    buildingUnitVersion.Addresses.Single().AddressPersistentLocalId.Should().Be(addressPersistentLocalId);
                    buildingUnitVersion.Addresses.Single().Position.Should().Be(position);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAdded()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingPersistentLocalId = (int) _fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var commonBuildingUnitWasAdded = _fixture.Create<CommonBuildingUnitWasAdded>();

            AddBuildingPersistentLocalId(buildingPersistentLocalId);
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);

            var position = _fixture.Create<long>();

            var commonBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(
                    new Envelope<CommonBuildingUnitWasAdded>(new Envelope(commonBuildingUnitWasAdded, commonBuildingUnitWasAddedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingId.Should().Be(commonBuildingUnitWasAdded.BuildingId);
                    buildingUnitVersion.BuildingUnitId.Should().Be(commonBuildingUnitWasAdded.BuildingUnitId);
                    buildingUnitVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalId);
                    buildingUnitVersion.BuildingUnitPersistentLocalId.Should().Be(buildingUnitPersistentLocalId);
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Common.Function);
                    buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Common.Map());
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(commonBuildingUnitWasAdded.Provenance.Timestamp);
                    buildingUnitVersion.VersionTimestamp.Should().Be(commonBuildingUnitWasAdded.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitPersistentLocalId}");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasRemoved = _fixture.Create<BuildingUnitWasRemoved>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasRemoved>(new Envelope(buildingUnitWasRemoved, buildingUnitWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.IsRemoved.Should().BeTrue();
                    buildingUnitVersion.Addresses.Should().BeEmpty();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasAttached()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();
            var attachedAddressPersistentLocalId = 2;

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttached>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);
            AddAddressPersistentLocalId(buildingUnitAddressWasAttached.AddressId, attachedAddressPersistentLocalId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttached>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Addresses.Should().HaveCount(2);
                    buildingUnitVersion.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == attachedAddressPersistentLocalId).Should().NotBeNull();

                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitAddressWasAttached.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetached()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitAddressWasDetached = new BuildingUnitAddressWasDetached(
                new BuildingId(buildingUnitWasAdded.BuildingId),
                new AddressId(buildingUnitWasAdded.AddressId),
                new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId));
            ((ISetProvenance)buildingUnitAddressWasDetached).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetached>(new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasAttachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Addresses.Should().BeEmpty();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasReaddressed()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var newAddressId = new AddressId(Guid.NewGuid());
            var newAddressPersistentLocalId = 2;
            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(
                new BuildingId(buildingUnitWasAdded.BuildingId),
                new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId),
                new AddressId(buildingUnitWasAdded.AddressId),
                newAddressId,
                new ReaddressingBeginDate(_fixture.Create<LocalDate>()));
            ((ISetProvenance)buildingUnitWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);
            AddAddressPersistentLocalId(newAddressId, newAddressPersistentLocalId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasReaddressedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasReaddressed>(new Envelope(buildingUnitWasReaddressed, buildingUnitWasReaddressedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Addresses.Should().ContainSingle();
                    buildingUnitVersion.Addresses.Single().AddressPersistentLocalId.Should().Be(newAddressPersistentLocalId);

                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasReaddressed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasAppointedByAdministrator()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPositionWasAppointedByAdministrator = _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPositionWasAppointedByAdministratorMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPositionWasAppointedByAdministrator>(
                        new Envelope(buildingUnitPositionWasAppointedByAdministrator, buildingUnitPositionWasAppointedByAdministratorMetadata))
                    )
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasAppointedByAdministrator.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.AppointedByAdministrator.GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitPositionWasAppointedByAdministrator.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasCorrectedToAppointedByAdministrator()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPositionWasCorrectedToAppointedByAdministrator = _fixture.Create<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPositionWasCorrectedToAppointedByAdministratorMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>(
                        new Envelope(buildingUnitPositionWasCorrectedToAppointedByAdministrator, buildingUnitPositionWasCorrectedToAppointedByAdministratorMetadata))
                    )
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasCorrectedToAppointedByAdministrator.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.AppointedByAdministrator.GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitPositionWasCorrectedToAppointedByAdministrator.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasCorrectedToDerivedFromObject()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPositionWasCorrectedToDerivedFromObject = _fixture.Create<BuildingUnitPositionWasCorrectedToDerivedFromObject>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPositionWasCorrectedToDerivedFromObjectMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>(
                        new Envelope(buildingUnitPositionWasCorrectedToDerivedFromObject, buildingUnitPositionWasCorrectedToDerivedFromObjectMetadata))
                    )
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasCorrectedToDerivedFromObject.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitPositionWasCorrectedToDerivedFromObject.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasDerivedFromObject()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPositionWasDerivedFromObject = _fixture.Create<BuildingUnitPositionWasDerivedFromObject>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPositionWasDerivedFromObjectMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPositionWasDerivedFromObject>(
                        new Envelope(buildingUnitPositionWasDerivedFromObject, buildingUnitPositionWasDerivedFromObjectMetadata))
                    )
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasDerivedFromObject.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitPositionWasDerivedFromObject.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitStatusWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasRealized = _fixture.Create<BuildingUnitWasRealized>();
            var buildingUnitStatusWasRemoved = _fixture.Create<BuildingUnitStatusWasRemoved>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitPositionWasDerivedFromObjectMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasRealized>(new Envelope(buildingUnitWasRealized, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitStatusWasRemoved>(new Envelope(buildingUnitStatusWasRemoved, buildingUnitPositionWasDerivedFromObjectMetadata))
                    )
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().BeNull();
                    buildingUnitVersion.OsloStatus.Should().BeNull();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitStatusWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealized()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasRealized = _fixture.Create<BuildingUnitWasRealized>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasRealized>(new Envelope(buildingUnitWasRealized, buildingUnitWasRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Realized");
                    buildingUnitVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetired()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasRetired = _fixture.Create<BuildingUnitWasRetired>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasRetired>(new Envelope(buildingUnitWasRetired, buildingUnitWasRetiredMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Retired");
                    buildingUnitVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredByParent()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasRetiredByParent = _fixture.Create<BuildingUnitWasRetiredByParent>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRetiredByParentMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasRetiredByParent>(new Envelope(buildingUnitWasRetiredByParent, buildingUnitWasRetiredByParentMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Retired");
                    buildingUnitVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRetiredByParent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasNotRealized = _fixture.Create<BuildingUnitWasNotRealized>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasNotRealized>(new Envelope(buildingUnitWasNotRealized, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("NotRealized");
                    buildingUnitVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedByParent()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasNotRealizedByParent = _fixture.Create<BuildingUnitWasNotRealizedByParent>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasNotRealizedByParentMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedByParent>(new Envelope(buildingUnitWasNotRealizedByParent, buildingUnitWasNotRealizedByParentMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("NotRealized");
                    buildingUnitVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealizedByParent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlanned()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasPlanned = _fixture.Create<BuildingUnitWasPlanned>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasPlanned>(new Envelope(buildingUnitWasPlanned, buildingUnitWasPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Planned");
                    buildingUnitVersion.OsloStatus.Should().Be("Gepland");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedToRealized()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasCorrectedToRealized = _fixture.Create<BuildingUnitWasCorrectedToRealized>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasCorrectedToRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedToRealized>(new Envelope(buildingUnitWasCorrectedToRealized, buildingUnitWasCorrectedToRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Realized");
                    buildingUnitVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedToNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasCorrectedToNotRealized = _fixture.Create<BuildingUnitWasCorrectedToNotRealized>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasCorrectedToNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedToNotRealized>(new Envelope(buildingUnitWasCorrectedToNotRealized, buildingUnitWasCorrectedToNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("NotRealized");
                    buildingUnitVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedToRetired()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasCorrectedToRetired = _fixture.Create<BuildingUnitWasCorrectedToRetired>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasCorrectedToRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedToRetired>(new Envelope(buildingUnitWasCorrectedToRetired, buildingUnitWasCorrectedToRetiredMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Retired");
                    buildingUnitVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasCorrectedToPlanned = _fixture.Create<BuildingUnitWasCorrectedToPlanned>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedToPlanned>(new Envelope(buildingUnitWasCorrectedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Planned");
                    buildingUnitVersion.OsloStatus.Should().Be("Gepland");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
                });
        }

        private void AddBuildingUnitPersistentLocalId(Guid buildingUnitId, int buildingUnitPersistentLocalId)
        {
            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);
        }

        private void AddBuildingUnitPersistentLocalId(int buildingUnitPersistentLocalId)
        {
            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);
        }

        private void AddBuildingPersistentLocalId(int? buildingPersistentLocalId = null)
        {
            var persistentLocalId = buildingPersistentLocalId ?? (int) _fixture.Create<BuildingPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(persistentLocalId);
        }

        private void AddAddressPersistentLocalId(Guid addressId, int addressPersistentLocalId = 1)
        {
            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(addressId))
                .ReturnsAsync(addressPersistentLocalId);
        }
    }
}
