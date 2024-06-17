namespace BuildingRegistry.Projections.Integration.Building.VersionFromMigration
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;

    public sealed partial class BuildingVersionProjections
    {
        private void RegisterBuildingUnitEvents(
            IntegrationOptions options)
        {
            var wkbReader = WKBReaderFactory.Create();

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            Status = BuildingUnitStatus.Planned.Status,
                            OsloStatus = BuildingUnitStatus.Planned.Map(),
                            Function = BuildingUnitFunction.Parse(message.Message.Function).Function,
                            OsloFunction = BuildingUnitFunction.Parse(message.Message.Function).Map(),
                            GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).GeometryMethod,
                            OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                            Geometry = sysGeometry,
                            HasDeviation = message.Message.HasDeviation,
                            IsRemoved = false,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
                            Type = message.EventName
                        };

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.IsRemoved = true;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;

                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.IsRemoved = true;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;

                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map();
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function).Function;
                        buildingUnit.OsloFunction = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).GeometryMethod;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        buildingUnit.IsRemoved = false;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;

                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.HasDeviation = false;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.HasDeviation = true;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.HasDeviation = true;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.HasDeviation = false;
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Status,
                            OsloStatus = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map(),
                            Function = BuildingUnitFunction.Common.Function,
                            OsloFunction = BuildingUnitFunction.Common.Map(),
                            GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).GeometryMethod,
                            OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                            Geometry = sysGeometry,
                            HasDeviation = message.Message.HasDeviation,
                            IsRemoved = false,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
                            Type = message.EventName
                        };

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).GeometryMethod;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            Position = message.Position
                        });

                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);

                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);

                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);

                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);

                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        var previousAddress = buildingUnit.Addresses
                            .SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);
                        if (previousAddress is not null && previousAddress.Count == 1)
                        {
                            buildingUnit.Addresses.Remove(previousAddress);
                        }
                        else if (previousAddress is not null)
                        {
                            previousAddress.Count -= 1;
                        }

                        var newAddress = buildingUnit.Addresses
                            .SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId);

                        if (newAddress is null)
                        {
                            buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                            {
                                AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId,
                                BuildingUnitPersistentLocalId = message.Message.BuildingPersistentLocalId,
                                Position = message.Position
                            });
                        }
                        else
                        {
                            newAddress.Count += 1;
                        }

                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    await context.CreateNewBuildingVersion(
                        message.Message.BuildingPersistentLocalId,
                        message,
                        building =>
                        {
                            var buildingUnit = building.BuildingUnits.Single(x =>
                                x.BuildingUnitPersistentLocalId == buildingUnitReaddresses.BuildingUnitPersistentLocalId);

                            foreach (var addressPersistentLocalId in buildingUnitReaddresses.DetachedAddressPersistentLocalIds)
                            {
                                var address = buildingUnit.Addresses.SingleOrDefault(x =>
                                    x.AddressPersistentLocalId == addressPersistentLocalId);
                                if (address is not null)
                                {
                                    buildingUnit.Addresses.Remove(address);
                                }
                            }

                            foreach (var addressPersistentLocalId in buildingUnitReaddresses.AttachedAddressPersistentLocalIds)
                            {
                                buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                                {
                                    AddressPersistentLocalId = addressPersistentLocalId,
                                    BuildingUnitPersistentLocalId = message.Message.BuildingPersistentLocalId,
                                    Position = message.Position
                                });
                            }

                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);
                        var status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                        var function = BuildingUnitFunction.Parse(message.Message.Function);
                        var geometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);

                        var buildingUnitVersion = new BuildingUnitVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            Status = status.Status,
                            OsloStatus = status.Map(),
                            Function = function.Function,
                            OsloFunction = function.Map(),
                            Geometry = sysGeometry,
                            GeometryMethod = geometryMethod.GeometryMethod,
                            OsloGeometryMethod = geometryMethod.Map(),
                            HasDeviation = message.Message.HasDeviation,
                            IsRemoved = false,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
                            Type = message.EventName
                        };

                        var addresses = message.Message.AddressPersistentLocalIds
                            .Distinct()
                            .Select(x => new BuildingUnitAddressVersion
                            {
                                Position = message.Position,
                                BuildingUnitPersistentLocalId = buildingUnitVersion.BuildingUnitPersistentLocalId,
                                AddressPersistentLocalId = x,
                            })
                            .ToList();

                        buildingUnitVersion.Addresses = new Collection<BuildingUnitAddressVersion>(addresses);

                        building.BuildingUnits.Add(buildingUnitVersion);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        var buildingUnit = building.BuildingUnits.Single(x =>
                            x.BuildingUnitPersistentLocalId == message.Message.BuildingUnitPersistentLocalId);

                        building.BuildingUnits.Remove(buildingUnit);
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });
        }
    }
}
