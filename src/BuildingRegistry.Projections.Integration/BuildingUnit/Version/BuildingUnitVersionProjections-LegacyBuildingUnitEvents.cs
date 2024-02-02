namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
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

    public sealed partial class BuildingUnitVersionProjections
    {
        private void RegisterLegacyBuildingUnitEvents(
            IntegrationOptions options,
            IPersistentLocalIdFinder persistentLocalIdFinder,
            IAddresses addresses,
            WKBReader wkbReader)
        {
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit => { buildingUnit.BuildingUnitPersistentLocalId = message.Message.PersistentLocalId; },
                    ct);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var buildingPersistentLocalId = await persistentLocalIdFinder.FindBuildingPersistentLocalId(
                    message.Message.BuildingId, ct);

                if (buildingPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building persistent local id found for {message.Message.BuildingId}");
                }

                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId, ct);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                var addressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No address persistent local id found for {message.Message.AddressId}");
                }

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    BuildingUnitId = message.Message.BuildingUnitId,
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
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
                    })
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                var buildingPersistentLocalId = await persistentLocalIdFinder.FindBuildingPersistentLocalId(
                    message.Message.BuildingId, ct);

                if (buildingPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building persistent local id found for {message.Message.BuildingId}");
                }

                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId, ct);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                var addressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No address persistent local id found for {message.Message.AddressId}");
                }

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    BuildingUnitId = message.Message.BuildingUnitId,
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
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
                    })
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var buildingPersistentLocalId = await persistentLocalIdFinder.FindBuildingPersistentLocalId(
                    message.Message.BuildingId, ct);

                if (buildingPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building persistent local id found for {message.Message.BuildingId}");
                }

                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId, ct);

                if (buildingUnitPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No building unit persistent local id found for {message.Message.BuildingUnitId}");
                }

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    BuildingUnitId = message.Message.BuildingUnitId,
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
                    Function = BuildingUnitFunction.Common.Function,
                    OsloFunction = BuildingUnitFunction.Common.Map(),
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.BuildingUnitNamespace,
                    PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}"
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                var building = await context.LatestPosition(message.Message.BuildingId, ct);

                if (building is null)
                {
                    throw new InvalidOperationException($"No building found for {message.Message.BuildingId}");
                }

                var buildingUnitStatus = building.Status == BuildingStatus.NotRealized
                    ? BuildingUnitStatus.NotRealized
                    : BuildingUnitStatus.Retired;
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = buildingUnitStatus;
                        buildingUnit.OsloStatus = buildingUnitStatus.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                        buildingUnit.Addresses.Clear();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                var addressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No address persistent local id found for {message.Message.AddressId}");
                }

                await context.CreateNewBuildingUnitVersion(
                    message.Message.To,
                    message,
                    buildingUnit =>
                    {
                        if (buildingUnit.Addresses.Any(x => x.AddressPersistentLocalId == addressPersistentLocalId.Value))
                        {
                            return;
                        }

                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = addressPersistentLocalId.Value
                        });
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.From,
                    message,
                    buildingUnit =>
                    {
                        foreach (var addressId in message.Message.AddressIds)
                        {
                            var addressPersistentLocalId = addresses.GetAddressPersistentLocalId(addressId).GetAwaiter().GetResult();

                            if (addressPersistentLocalId is null)
                            {
                                throw new InvalidOperationException($"No address persistent local id found for {addressId}");
                            }

                            var address = buildingUnit.Addresses
                                .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId.Value);

                            if (address is not null)
                            {
                                buildingUnit.Addresses.Remove(address);
                            }
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
            {
                var oldAddressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.OldAddressId);

                if (oldAddressPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No address persistent local id found for {message.Message.OldAddressId}");
                }

                var newAddressPersistentLocalId = await addresses.GetAddressPersistentLocalId(message.Message.NewAddressId);

                if (newAddressPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No address persistent local id found for {message.Message.NewAddressId}");
                }

                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        var oldAddress =
                            buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == oldAddressPersistentLocalId);
                        if (oldAddress is not null)
                        {
                            buildingUnit.Addresses.Remove(oldAddress);
                        }

                        if (buildingUnit.Addresses.All(x => x.AddressPersistentLocalId != newAddressPersistentLocalId))
                        {
                            buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                            {
                                Position = message.Position,
                                BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                                AddressPersistentLocalId = newAddressPersistentLocalId.Value
                            });
                        }
                    },
                    ct);
            });

            #region Position

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.GeometryMethod;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.GeometryMethod;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                    },
                    ct);
            });

            #endregion

            #region Status

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = null;
                        buildingUnit.OsloStatus = null;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            #endregion
        }
    }
}
