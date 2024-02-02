namespace BuildingRegistry.Projections.Integration.Building.Version
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
                            Status =  BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Status,
                            OsloStatus =  BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map(),
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
                            PuriId = $"{options.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}"
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

                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }

                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingPersistentLocalId,
                            Position = message.Position
                        });

                        buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
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

            When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
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
                            Status = BuildingUnitStatus.Parse(message.Message.Status).Status,
                            OsloStatus = BuildingUnitStatus.Parse(message.Message.Status).Map(),
                            Function = BuildingUnitFunction.Parse(message.Message.Function).Function,
                            OsloFunction = BuildingUnitFunction.Parse(message.Message.Function).Map(),
                            Geometry = sysGeometry,
                            GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).GeometryMethod,
                            OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                            HasDeviation = message.Message.HasDeviation,
                            IsRemoved = false,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.BuildingUnitNamespace,
                            PuriId = $"{options.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
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

            When<Envelope<BuildingUnitWasMoved>>(async (context, message, ct) =>
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
