namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class MeasureBuildingRequest
    {
        public GrbData GrbData { get; set; }

        public MeasureBuilding ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new MeasureBuilding(
                buildingPersistentLocalId,
                GrbData.GeometriePolygoon.ToExtendedWkbGeometry(),
                GrbData.ToBuildingGrbData(),
                provenance);
        }
    }
}
