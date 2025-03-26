namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Building;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus Map(this BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static GebouweenheidFunctie Map(this BuildingUnitFunction function)
        {
            if (BuildingUnitFunction.Common == function)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            if (BuildingUnitFunction.Unknown == function)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }
    }

    public static class BuildingUnitPositionGeometryMethodExtensions
    {
        public static PositieGeometrieMethode Map(this BuildingUnitPositionGeometryMethod geometryMethod)
        {
            if (BuildingUnitPositionGeometryMethod.AppointedByAdministrator == geometryMethod)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            if (BuildingUnitPositionGeometryMethod.DerivedFromObject == geometryMethod)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }
    }

    public static class BuildingUnitDetailExtensions
    {
        public static async Task FindAndUpdateBuildingUnit(
            this ProducerContext context,
            int persistentLocalId,
            Action<BuildingUnitDetail> updateFunc,
            CancellationToken ct)
        {
            var buildingUnit = await context
                .BuildingUnits
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (buildingUnit is null)
            {
                throw new ProjectionItemNotFoundException<ProducerProjections>(persistentLocalId.ToString());
            }

            updateFunc(buildingUnit);
        }
    }
}
