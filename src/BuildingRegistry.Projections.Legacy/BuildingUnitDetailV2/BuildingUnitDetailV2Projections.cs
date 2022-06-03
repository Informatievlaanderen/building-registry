namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Microsoft.EntityFrameworkCore;

    [ConnectedProjectionName("API endpoint detail/lijst gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het gebouweenheden detail & lijst voorziet.")]
    public class BuildingUnitDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public BuildingUnitDetailV2Projections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                BuildingStatus? retiredStatus = null;
                var buildingStatus = BuildingStatus.Parse(message.Message.BuildingStatus);
                if (buildingStatus == BuildingStatus.Retired || buildingStatus == BuildingStatus.NotRealized)
                {
                    retiredStatus = buildingStatus;
                }

                var building = new BuildingUnitBuildingItemV2(
                    message.Message.BuildingPersistentLocalId,
                    message.Message.IsRemoved,
                    retiredStatus);

                await context.BuildingUnitBuildingsV2.AddAsync(building, ct);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var addressesIdsGrouped = buildingUnit.AddressPersistentLocalIds.GroupBy(x => x);
                    var addresses = addressesIdsGrouped
                        .Select(groupedAddressId => new BuildingUnitDetailAddressItemV2
                        {
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = groupedAddressId.Key,
                            Count = groupedAddressId.Count()
                        })
                        .ToList();

                    await context.BuildingUnitDetailsV2.AddAsync(
                        new BuildingUnitDetailItemV2(
                            buildingUnit.BuildingUnitPersistentLocalId,
                            message.Message.BuildingPersistentLocalId,
                            buildingUnit.ExtendedWkbGeometry.ToByteArray(),
                            BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                            BuildingUnitFunction.Parse(buildingUnit.Function),
                            BuildingUnitStatus.Parse(buildingUnit.Status),
                            new Collection<BuildingUnitDetailAddressItemV2>(addresses),
                            buildingUnit.IsRemoved,
                            message.Message.Provenance.Timestamp)
                        , ct);
                }
            });
        }

        private static async Task<List<BuildingUnitDetailItemV2>> GetAllBuildingUnitsByBuildingPersistentLocalId(LegacyContext context, int buildingPersistentLocalId, CancellationToken ct)
        {
            return (await context.BuildingUnitDetailsV2.Where(unit => unit.BuildingPersistentLocalId == buildingPersistentLocalId).ToListAsync(ct))
                .Union(context.BuildingUnitDetailsV2.Local.Where(unit => unit.BuildingPersistentLocalId == buildingPersistentLocalId))
                .ToList();
        }
    }
}
