namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    [ConnectedProjectionName("Kafka producer ldes gebouwen")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly IProducer _buildingProducer;
        private readonly IProducer _buildingUnitProducer;
        private readonly string _buildingOsloNamespace;
        private readonly string _buildingUnitOsloNamespace;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ProducerProjections(
            IProducer buildingProducer,
            IProducer buildingUnitProducer,
            string buildingOsloNamespace,
            string buildingUnitOsloNamespace,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _buildingProducer = buildingProducer;
            _buildingUnitProducer = buildingUnitProducer;
            _buildingOsloNamespace = buildingOsloNamespace;
            _buildingUnitOsloNamespace = buildingUnitOsloNamespace;
            _jsonSerializerSettings = jsonSerializerSettings;

            var wkbReader = WKBReaderFactory.Create();

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var building = new BuildingDetail(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Parse(message.Message.GeometryMethod),
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingStatus.Parse(message.Message.BuildingStatus),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var addresses = buildingUnit.AddressPersistentLocalIds
                        .Distinct()
                        .Select(x => new BuildingUnitDetailAddress(buildingUnit.BuildingUnitPersistentLocalId, x))
                        .ToList();

                    var buildingUnitDetailItemV2 = new BuildingUnitDetail(
                        buildingUnit.BuildingUnitPersistentLocalId,
                        message.Message.BuildingPersistentLocalId,
                        buildingUnit.ExtendedWkbGeometry.ToByteArray(),
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                        BuildingUnitFunction.Parse(buildingUnit.Function),
                        BuildingUnitStatus.Parse(buildingUnit.Status),
                        false,
                        addresses,
                        buildingUnit.IsRemoved,
                        message.Message.Provenance.Timestamp);

                    await context.BuildingUnits.AddAsync(buildingUnitDetailItemV2, ct);
                }

                await context
                    .Buildings
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var building = new BuildingDetail(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Outlined,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingStatus.Planned,
                    false,
                    message.Message.Provenance.Timestamp);

                await context
                    .Buildings
                    .AddAsync(building, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var building = new BuildingDetail(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.MeasuredByGrb,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingStatus.Realized,
                    false,
                    message.Message.Provenance.Timestamp);

                await context
                    .Buildings
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Geometry = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        buildingUnit =>
                        {
                            buildingUnit.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                            buildingUnit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                            buildingUnit.Version = message.Message.Provenance.Timestamp;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Geometry = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        buildingUnit =>
                        {
                            buildingUnit.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                            buildingUnit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                            buildingUnit.Version = message.Message.Provenance.Timestamp;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.UnderConstruction;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.Planned;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.Realized;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.UnderConstruction;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.NotRealized;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.Planned;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Geometry = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        buildingUnitDetail =>
                        {
                            buildingUnitDetail.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                            buildingUnitDetail.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                            buildingUnitDetail.Version = message.Message.Provenance.Timestamp;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Geometry = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        buildingUnit =>
                        {
                            buildingUnit.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                            buildingUnit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                            buildingUnit.Version = message.Message.Provenance.Timestamp;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Status = BuildingStatus.Retired;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.IsRemoved = true;
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            #region BuildingUnit

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                // todo-pr produce building
                await context.FindAndUpdateBuilding(message.Message.BuildingPersistentLocalId, building =>
                {
                    building.Version = message.Message.Provenance.Timestamp;
                }, ct);

                var buildingUnitDetailItemV2 = new BuildingUnitDetail(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                    BuildingUnitFunction.Parse(message.Message.Function),
                    BuildingUnitStatus.Planned,
                    message.Message.HasDeviation,
                    [],
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                await context.BuildingUnits.AddAsync(buildingUnitDetailItemV2, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
                ;
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(message.Message.BuildingPersistentLocalId, building =>
                {
                    building.Version = message.Message.Provenance.Timestamp;
                }, ct);

                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(message.Message.BuildingPersistentLocalId, building =>
                {
                    building.Version = message.Message.Provenance.Timestamp;
                }, ct);

                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function);
                        buildingUnit.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                        buildingUnit.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                        buildingUnit.IsRemoved = false;
                        buildingUnit.Addresses = [];
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(message.Message.BuildingPersistentLocalId, building =>
                {
                    building.Version = message.Message.Provenance.Timestamp;
                }, ct);

                var buildingUnit = new BuildingUnitDetail(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                    BuildingUnitFunction.Common,
                    BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus),
                    message.Message.HasDeviation,
                    [],
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                await context.BuildingUnits.AddAsync(buildingUnit, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                        buildingUnit.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        buildingUnit.Addresses.Add(
                            new BuildingUnitDetailAddress(message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId));
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        var address = buildingUnit.Addresses.Single(x =>
                            x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        buildingUnit.Addresses.Remove(address);
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        var address = buildingUnit.Addresses.Single(x =>
                            x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        buildingUnit.Addresses.Remove(address);
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        var address = buildingUnit.Addresses.Single(x =>
                            x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        buildingUnit.Addresses.Remove(address);
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        var address = buildingUnit.Addresses.Single(x =>
                            x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        buildingUnit.Addresses.Remove(address);
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        var previousAddress = buildingUnit.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);

                        if (previousAddress is not null && previousAddress.Count == 1)
                        {
                            buildingUnit.Addresses.Remove(previousAddress);
                        }
                        else if (previousAddress is not null)
                        {
                            previousAddress.Count -= 1;
                        }

                        var newAddress = buildingUnit.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId);

                        if (newAddress is null)
                        {
                            buildingUnit.Addresses.Add(new BuildingUnitDetailAddress(
                                message.Message.BuildingUnitPersistentLocalId,
                                message.Message.NewAddressPersistentLocalId));
                        }
                        else
                        {
                            newAddress.Count += 1;
                        }

                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitReaddresses.BuildingUnitPersistentLocalId,
                        buildingUnit =>
                        {
                            context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                            foreach (var addressPersistentLocalId in buildingUnitReaddresses.DetachedAddressPersistentLocalIds)
                            {
                                RemoveIdempotentAddress(buildingUnit, addressPersistentLocalId);
                            }

                            foreach (var addressPersistentLocalId in buildingUnitReaddresses.AttachedAddressPersistentLocalIds)
                            {
                                AddIdempotentAddress(buildingUnit,
                                    new BuildingUnitDetailAddress(
                                        buildingUnitReaddresses.BuildingUnitPersistentLocalId,
                                        addressPersistentLocalId));
                            }

                            buildingUnit.Version = message.Message.Provenance.Timestamp;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        context.Entry(buildingUnit).Collection(x => x.Addresses).Load();

                        RemoveIdempotentAddress(buildingUnit, message.Message.PreviousAddressPersistentLocalId);

                        AddIdempotentAddress(buildingUnit,
                            new BuildingUnitDetailAddress(
                                message.Message.BuildingUnitPersistentLocalId,
                                message.Message.NewAddressPersistentLocalId));

                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);

                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;

                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function);
                        buildingUnit.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                        buildingUnit.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                        buildingUnit.IsRemoved = false;
                        buildingUnit.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuilding(
                    message.Message.BuildingPersistentLocalId,
                    building =>
                    {
                        building.Version = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            #endregion
        }

        private static void RemoveIdempotentAddress(BuildingUnitDetail buildingUnit, int addressPersistentLocalId)
        {
            var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (address is not null)
            {
                buildingUnit.Addresses.Remove(address);
            }
        }

        private static void AddIdempotentAddress(
            BuildingUnitDetail buildingUnit,
            BuildingUnitDetailAddress buildingUnitDetailAddress)
        {
            var address = buildingUnit.Addresses.SingleOrDefault(x =>
                x.AddressPersistentLocalId == buildingUnitDetailAddress.AddressPersistentLocalId);

            if (address is null)
            {
                buildingUnit.Addresses.Add(buildingUnitDetailAddress);
            }
        }

        private async Task ProduceBuilding(
            ProducerContext context,
            int buildingPersistentLocalId,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var building = await context.Buildings.FindAsync(buildingPersistentLocalId, cancellationToken: cancellationToken)
                         ?? throw new ProjectionItemNotFoundException<ProducerProjections>(buildingPersistentLocalId.ToString());

            var buildingUnitPersistentLocalIds = await context.BuildingUnits
                .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToListAsync(cancellationToken);

            var buildingLdes = new BuildingLdes(building, buildingUnitPersistentLocalIds, _buildingOsloNamespace);

            await ProduceBuilding(
                $"{_buildingOsloNamespace}/{building.PersistentLocalId}",
                building.PersistentLocalId.ToString(),
                JsonConvert.SerializeObject(buildingLdes, _jsonSerializerSettings),
                storePosition,
                cancellationToken);
        }

        private async Task ProduceBuilding(string puri, string objectId, string jsonContent, long storePosition,
            CancellationToken cancellationToken = default)
        {
            var result = await _buildingProducer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private async Task ProduceBuildingUnit(
            ProducerContext context,
            int buildingUnitPersistentLocalId,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var buildingUnit = await context.BuildingUnits
                                   .Include(x => x.Addresses)
                                   .SingleOrDefaultAsync(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId, cancellationToken)
                           ?? throw new ProjectionItemNotFoundException<ProducerProjections>(buildingUnitPersistentLocalId.ToString());

            var buildingUnitLdes = new BuildingUnitLdes(buildingUnit, _buildingUnitOsloNamespace);

            await ProduceBuilding(
                $"{_buildingUnitOsloNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}",
                buildingUnit.BuildingUnitPersistentLocalId.ToString(),
                JsonConvert.SerializeObject(buildingUnitLdes, _jsonSerializerSettings),
                storePosition,
                cancellationToken);
        }

        private async Task ProduceBuildingUnit(string puri, string objectId, string jsonContent, long storePosition,
            CancellationToken cancellationToken = default)
        {
            var result = await _buildingUnitProducer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private static Task DoNothing<T>(ProducerContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage =>
            Task.CompletedTask;
    }
}
