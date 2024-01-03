namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;
    using Legacy.Events;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie gebouweenheid latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste gebouweenheid data voor de integratie database bijhoudt.")]
    public sealed class BuildingUnitVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingUnitVersionProjections(
            IOptions<IntegrationOptions> options,
            IPersistentLocalIdFinder persistentLocalIdFinder)
        {
            var wkbReader = WKBReaderFactory.Create();

            #region Legacy

            #region Building

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitId in message.Message.BuildingUnitIds)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.IsRemoved = true;
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }

                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }

                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }

                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }

                foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                            buildingUnit.LegacyAddresses.Clear();
                            buildingUnit.Addresses.Clear();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnits = GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId);
                foreach (var buildingUnit in buildingUnits)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnit.BuildingUnitId!.Value,
                        message,
                        x =>
                        {
                            x.Geometry = null;
                            x.GeometryMethod = null;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var buildingUnits = GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId);
                foreach (var buildingUnit in buildingUnits)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnit.BuildingUnitId!.Value,
                        message,
                        _ => { },
                        ct);
                }
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnits = GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId);
                foreach (var buildingUnit in buildingUnits)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnit.BuildingUnitId!.Value,
                        message,
                        _ => { },
                        ct);
                }
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var buildingUnits = GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId);
                foreach (var buildingUnit in buildingUnits)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnit.BuildingUnitId!.Value,
                        message,
                        x =>
                        {
                            x.BuildingPersistentLocalId = message.Message.PersistentLocalId;
                        },
                        ct);
                }
            });
            #endregion

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
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
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
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}",
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
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{buildingUnitPersistentLocalId}"
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
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
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
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            #endregion

            #endregion

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var geometryAsBinary = buildingUnit.ExtendedWkbGeometry.ToByteArray();
                    var sysGeometry = wkbReader.Read(geometryAsBinary);

                    var addresses = buildingUnit.AddressPersistentLocalIds
                        .Distinct()
                        .Select(x => new BuildingUnitAddressVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = x,
                        })
                        .ToList();

                    var buildingUnitVersion = new BuildingUnitVersion
                    {
                        Position = message.Position,
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status).Map(),
                        Function = BuildingUnitFunction.Parse(buildingUnit.Function).Map(),
                        GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                        Geometry = sysGeometry,
                        HasDeviation = false,
                        IsRemoved = message.Message.IsRemoved,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        Namespace = options.Value.BuildingUnitNamespace,
                        PuriId = $"{options.Value.BuildingUnitNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}",
                        Addresses = new Collection<BuildingUnitAddressVersion>(addresses)
                    };

                    await context
                        .BuildingUnitVersions
                        .AddAsync(buildingUnitVersion, ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                    && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                    && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        },
                        ct);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                    && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        },
                        ct);
                }
            });

            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingUnitStatus.Planned.Map(),
                    Function = BuildingUnitFunction.Parse(message.Message.Function).Map(),
                    GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    HasDeviation = message.Message.HasDeviation,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map();
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        buildingUnit.IsRemoved = false;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                    },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status =  BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map(),
                    Function = BuildingUnitFunction.Common.Map(),
                    GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    HasDeviation = message.Message.HasDeviation,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}"
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            Position = message.Position
                        });
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }

                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            Position = message.Position
                        });

                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.Status).Map();
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                    },
                    ct);
            });

            // BuildingUnitWasTransferred couples the unit to another building and BuildingUnitMoved is an event applicable on the old building.
            When<Envelope<BuildingUnitWasMoved>>((_, _, _) => Task.CompletedTask);
        }

        private IEnumerable<BuildingUnitVersion> GetAllBuildingUnitsByBuildingId(
            IntegrationContext context,
            Guid buildingId)
        {
            var buildingUnits =
                context.BuildingUnitVersions
                    .Where(x => x.BuildingId == buildingId)
                    .ToList()
                    .Union(
                        context.BuildingUnitVersions.Local
                            .Where(x => x.BuildingId == buildingId)
                            .ToList());

            return buildingUnits;
        }
    }
}
