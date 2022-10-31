using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;

namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    public class CorrectBuildingUnitPositionBackOfficeRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        public string? Positie { get; set; }
    }
}
