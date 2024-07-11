#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using Converters;
    using Infrastructure;
    using Legacy.Events;
    using NetTopologySuite.IO;
    using IAddresses = IAddresses;

    public sealed partial class BuildingVersionProjections
    {
        private const int InvalidAddressPersistentLocalId = -1;

        private void RegisterLegacyBuildingUnitEvents(
            IntegrationOptions options,
            IPersistentLocalIdFinder persistentLocalIdFinder,
            IAddresses addresses,
            WKBReader wkbReader)
        {
            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                var addressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    await addresses.AddAddressPersistentLocalId(message.Message.AddressId, InvalidAddressPersistentLocalId);
                    addressPersistentLocalId = InvalidAddressPersistentLocalId;
                    //throw new InvalidOperationException($"No address persistent local id found for {message.Message.AddressId}");
                }

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                            BuildingPersistentLocalId = building.BuildingPersistentLocalId,
                            Function = BuildingUnitFunction.Unknown.Function,
                            OsloFunction = BuildingUnitFunction.Unknown.Map(),
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
                            Addresses = new Collection<BuildingUnitAddressVersion>(new[]
                            {
                                new BuildingUnitAddressVersion
                                {
                                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                                    AddressPersistentLocalId = addressPersistentLocalId.Value,
                                    Position = message.Position
                                }
                            }),
                            Type = message.EventName
                        };

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits
                            .SingleOrDefault(x => x.BuildingUnitId == message.Message.BuildingUnitId);

                        if (buildingUnit is not null)
                        {
                            buildingUnit.BuildingUnitPersistentLocalId = message.Message.PersistentLocalId;
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                var addressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    await addresses.AddAddressPersistentLocalId(message.Message.AddressId, InvalidAddressPersistentLocalId);
                    addressPersistentLocalId = InvalidAddressPersistentLocalId;
                    //throw new InvalidOperationException($"No address persistent local id found for {message.Message.AddressId}");
                }

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                            BuildingPersistentLocalId = building.BuildingPersistentLocalId,
                            Function = BuildingUnitFunction.Unknown.Function,
                            OsloFunction = BuildingUnitFunction.Unknown.Map(),
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
                            Addresses = new Collection<BuildingUnitAddressVersion>(new[]
                            {
                                new BuildingUnitAddressVersion
                                {
                                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                                    AddressPersistentLocalId = addressPersistentLocalId.Value,
                                    Position = message.Position
                                }
                            }),
                            Type = message.EventName
                        };

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                            BuildingPersistentLocalId = building.BuildingPersistentLocalId,
                            Function = BuildingUnitFunction.Common.Function,
                            OsloFunction = BuildingUnitFunction.Common.Map(),
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
                            Type = message.EventName
                        };

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitStatus = building.Status == BuildingStatus.NotRealized
                            ? BuildingUnitStatus.NotRealized
                            : BuildingUnitStatus.Retired;

                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                            BuildingPersistentLocalId = building.BuildingPersistentLocalId,
                            Status = buildingUnitStatus,
                            OsloStatus = buildingUnitStatus.Map(),
                            Function = BuildingUnitFunction.Unknown.Function,
                            OsloFunction = BuildingUnitFunction.Unknown.Map(),
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
                            Type = message.EventName
                        };

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion =
                            building.BuildingUnits.Single(x => x.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.IsRemoved = true;
                        buildingUnitVersion.Addresses.Clear();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;

                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                var addressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    await addresses.AddAddressPersistentLocalId(message.Message.AddressId, InvalidAddressPersistentLocalId);
                    addressPersistentLocalId = InvalidAddressPersistentLocalId;
                    //throw new InvalidOperationException($"No address persistent local id found for {message.Message.AddressId}");
                }

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion =
                            building.BuildingUnits.Single(x => x.BuildingUnitId == message.Message.To);

                        var buildingUnitAddress =
                            buildingUnitVersion.Addresses.FirstOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId.Value);

                        if (buildingUnitAddress is not null)
                        {
                            buildingUnitAddress.Count++;
                        }
                        else
                        {
                            buildingUnitVersion.Addresses.Add(new BuildingUnitAddressVersion
                            {
                                Position = message.Position,
                                BuildingUnitPersistentLocalId = buildingUnitVersion.BuildingUnitPersistentLocalId,
                                AddressPersistentLocalId = addressPersistentLocalId.Value
                            });
                        }

                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message, building =>
                    {
                        var buildingUnitVersion =
                            building.BuildingUnits.Single(x => x.BuildingUnitId == message.Message.From);

                        var changed = false;
                        foreach (var addressId in message.Message.AddressIds)
                        {
                            var addressPersistentLocalId = addresses.GetAddressPersistentLocalId(addressId).GetAwaiter().GetResult();

                            if (addressPersistentLocalId is null)
                            {
                                addresses.AddAddressPersistentLocalId(addressId, InvalidAddressPersistentLocalId).GetAwaiter().GetResult();
                                addressPersistentLocalId = InvalidAddressPersistentLocalId;
                                //throw new InvalidOperationException($"No address persistent local id found for {addressId}");
                            }

                            var address = buildingUnitVersion.Addresses
                                .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId.Value);

                            if (address is null)
                            {
                                continue;
                            }

                            if (address.Count > 1)
                            {
                                address.Count--;
                            }
                            else
                            {
                                buildingUnitVersion.Addresses.Remove(address);
                            }

                            changed = true;
                        }

                        if (changed)
                        {
                            buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion
                            .Readdresses
                            .Add(new BuildingUnitReaddressVersion
                            {
                                Position = message.Position,
                                BuildingUnitPersistentLocalId = buildingUnitVersion.BuildingUnitPersistentLocalId,
                                OldAddressId = message.Message.OldAddressId,
                                NewAddressId = message.Message.NewAddressId,
                                ReaddressBeginDate = message.Message.BeginDate
                            });
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;

                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            #region Position

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnitVersion.Geometry = sysGeometry;
                        buildingUnitVersion.GeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.GeometryMethod;
                        buildingUnitVersion.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnitVersion.Geometry = sysGeometry;
                        buildingUnitVersion.GeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.GeometryMethod;
                        buildingUnitVersion.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnitVersion.Geometry = sysGeometry;
                        buildingUnitVersion.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                        buildingUnitVersion.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnitVersion.Geometry = sysGeometry;
                        buildingUnitVersion.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                        buildingUnitVersion.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            #endregion

            #region Status

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = null;
                        buildingUnitVersion.OsloStatus = null;
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        var buildingUnitVersion = building.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        buildingUnitVersion.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnitVersion.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            #endregion
        }
    }
}
