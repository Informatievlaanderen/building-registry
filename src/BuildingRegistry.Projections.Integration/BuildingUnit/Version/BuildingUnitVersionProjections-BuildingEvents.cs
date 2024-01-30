namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;
    using NetTopologySuite.IO;

    public sealed partial class BuildingUnitVersionProjections
    {
        private void RegisterBuildingEvents(IntegrationOptions options, WKBReader wkbReader)
        {
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
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status).Status,
                        OsloStatus = BuildingUnitStatus.Parse(buildingUnit.Status).Map(),
                        Function = BuildingUnitFunction.Parse(buildingUnit.Function).Function,
                        OsloFunction = BuildingUnitFunction.Parse(buildingUnit.Function).Map(),
                        GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).Map(),
                        Geometry = sysGeometry,
                        HasDeviation = false,
                        IsRemoved = buildingUnit.IsRemoved,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                        Namespace = options.BuildingUnitNamespace,
                        PuriId = $"{options.BuildingUnitNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}",
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
        }
    }
}
