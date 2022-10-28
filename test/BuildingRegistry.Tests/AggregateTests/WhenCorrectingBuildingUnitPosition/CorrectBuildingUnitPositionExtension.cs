namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitPosition
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using Building;
    using Building.Commands;

    public static class CorrectBuildingUnitPositionExtension
    {
        public static CorrectBuildingUnitPosition WithPositionGeometryMethod(this CorrectBuildingUnitPosition cmd, BuildingUnitPositionGeometryMethod method)
        {
            return new CorrectBuildingUnitPosition(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                method, cmd.Position, cmd.Provenance);
        }

        public static CorrectBuildingUnitPosition WithPointPosition(this CorrectBuildingUnitPosition cmd, string? point)
        {
            var positionExtendedWkbGeometry = !string.IsNullOrEmpty(point)
                ? point.ToExtendedWkbGeometry()
                : null;

            return new CorrectBuildingUnitPosition(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId,
                cmd.PositionGeometryMethod, positionExtendedWkbGeometry, cmd.Provenance);
        }

        public static CorrectBuildingUnitPosition WithPersistentLocalId(this CorrectBuildingUnitPosition cmd,
            BuildingUnitPersistentLocalId persistentLocalId)
        {
            return new CorrectBuildingUnitPosition(cmd.BuildingPersistentLocalId, persistentLocalId,
                cmd.PositionGeometryMethod, cmd.Position, cmd.Provenance);
        }
    }
}
