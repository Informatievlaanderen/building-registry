namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using BuildingRegistry.Building;
    using Converters;
    using Legacy.Events;

    public sealed partial class BuildingUnitVersionProjections
    {
        private void RegisterLegacyBuildingEvents()
        {
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
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
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
                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
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
                            x.OsloGeometryMethod = null;
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
