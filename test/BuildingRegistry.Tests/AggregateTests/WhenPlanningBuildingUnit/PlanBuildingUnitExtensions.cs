namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
    }
}
