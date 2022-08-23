namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
{
    using Api.BackOffice.Abstractions.Building;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;

    public static class PlanBuildingUnitExtensions
    {
        public static PlanBuildingUnit WithDeviation(this PlanBuildingUnit cmd, bool deviation)
        {
            return new PlanBuildingUnit(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                cmd.PositionGeometryMethod, cmd.Position, cmd.Function, deviation, cmd.Provenance);
        }

        public static PlanBuildingUnit WithoutPosition(this PlanBuildingUnit cmd)
        {
            return new PlanBuildingUnit(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                cmd.PositionGeometryMethod, null, cmd.Function, true, cmd.Provenance);
        }

        public static PlanBuildingUnit WithProvenance(this PlanBuildingUnit cmd, Provenance provenance)
        {
            return new PlanBuildingUnit(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                cmd.PositionGeometryMethod, cmd.Position, cmd.Function, cmd.HasDeviation, provenance);
        }

        public static PlanBuildingUnit WithPositionGeometryMethod(this PlanBuildingUnit cmd, BuildingUnitPositionGeometryMethod method)
        {
            return new PlanBuildingUnit(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                method, cmd.Position, cmd.Function, cmd.HasDeviation, cmd.Provenance);
        }

        public static PlanBuildingUnit WithPointPosition(this PlanBuildingUnit cmd, string? point)
        {
            var positionExtendedWkbGeometry = !string.IsNullOrEmpty(point)
                ? point.ToExtendedWkbGeometry()
                : null;

            return new PlanBuildingUnit(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                cmd.PositionGeometryMethod, positionExtendedWkbGeometry, cmd.Function, cmd.HasDeviation, cmd.Provenance);
        }

        public static PlanBuildingUnit WithPersistentLocalId(this PlanBuildingUnit cmd,
            BuildingUnitPersistentLocalId persistentLocalId)
        {
            return new PlanBuildingUnit(cmd.BuildingPersistentLocalId, persistentLocalId,
                cmd.PositionGeometryMethod, cmd.Position, cmd.Function, cmd.HasDeviation, cmd.Provenance);
        }
    }
}
