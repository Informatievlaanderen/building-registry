namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using Converters;
    using Infrastructure;
    using Legacy.Events;
    using NetTopologySuite.IO;

    public sealed partial class BuildingUnitVersionProjections
    {
        private void RegisterLegacyBuildingUnitEvents(
            IntegrationOptions options,
            IPersistentLocalIdFinder persistentLocalIdFinder,
            WKBReader wkbReader)
        {
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.BuildingUnitPersistentLocalId = message.Message.PersistentLocalId;
                    },
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
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId, ct);

                if (buildingPersistentLocalId is null || buildingUnitPersistentLocalId is null)
                {
                    return;
                }
                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    BuildingUnitId = message.Message.BuildingUnitId,
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
                    Function = BuildingUnitFunction.Unknown.Map(),
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.BuildingUnitNamespace,
                    PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
                    LegacyAddresses = new Collection<BuildingUnitAddressLegacyVersion>(new[]
                    {
                        new BuildingUnitAddressLegacyVersion
                        {
                            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                            AddressId = message.Message.AddressId,
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
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId, ct);

                if (buildingPersistentLocalId is null || buildingUnitPersistentLocalId is null)
                {
                    return;
                }
                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    BuildingUnitId = message.Message.BuildingUnitId,
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
                    Function = BuildingUnitFunction.Unknown.Map(),
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.BuildingUnitNamespace,
                    PuriId = $"{options.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
                    LegacyAddresses = new Collection<BuildingUnitAddressLegacyVersion>(new[]
                    {
                        new BuildingUnitAddressLegacyVersion
                        {
                            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                            AddressId = message.Message.AddressId,
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
                var buildingUnitPersistentLocalId = await persistentLocalIdFinder.FindBuildingUnitPersistentLocalId(
                    message.Message.BuildingId, message.Message.BuildingUnitId, ct);

                if (buildingPersistentLocalId is null || buildingUnitPersistentLocalId is null)
                {
                    return;
                }
                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    BuildingUnitId = message.Message.BuildingUnitId,
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId.Value,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
                    Function = BuildingUnitFunction.Common.Map(),
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
                // await AddUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, null, message.Message.Provenance.Timestamp, false, ct);
                // var addedUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                // var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                //
                // addedUnit.Status = building.BuildingRetiredStatus == BuildingStatus.NotRealized
                //     ? BuildingUnitStatus.NotRealized
                //     : BuildingUnitStatus.Retired;
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                        buildingUnit.LegacyAddresses.Clear();
                        buildingUnit.Addresses.Clear();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.To,
                    message,
                    buildingUnit =>
                    {
                        if (buildingUnit.LegacyAddresses.Any(x => x.AddressId == message.Message.AddressId))
                        {
                            return;
                        }

                        buildingUnit.LegacyAddresses.Add(new BuildingUnitAddressLegacyVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressId = message.Message.AddressId
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
                            var address = buildingUnit.LegacyAddresses.SingleOrDefault(x => x.AddressId == addressId);
                            if (address is not null)
                            {
                                buildingUnit.LegacyAddresses.Remove(address);
                            }
                        }
                    },
                    ct);

            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitId,
                    message,
                    buildingUnit =>
                    {
                        var oldAddress =
                            buildingUnit.LegacyAddresses.SingleOrDefault(x => x.AddressId == message.Message.OldAddressId);
                        if (oldAddress is not null)
                        {
                            buildingUnit.LegacyAddresses.Remove(oldAddress);
                        }

                        if (buildingUnit.LegacyAddresses.All(x => x.AddressId != message.Message.NewAddressId))
                        {
                            buildingUnit.LegacyAddresses.Add(new BuildingUnitAddressLegacyVersion
                            {
                                Position = message.Position,
                                BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                                AddressId = message.Message.NewAddressId
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
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map();
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
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator.Map();
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
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
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
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
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
