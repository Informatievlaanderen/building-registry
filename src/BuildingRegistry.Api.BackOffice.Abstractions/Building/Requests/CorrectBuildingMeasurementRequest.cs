namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class CorrectBuildingMeasurementRequest
    {
        public GrbData GrbData { get; set; }

        public CorrectBuildingMeasurement ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new CorrectBuildingMeasurement(
                buildingPersistentLocalId,
                GrbData.GeometriePolygoon.ToExtendedWkbGeometry(),
                GrbData.ToBuildingGrbData(),
                provenance);
        }
    }
}
