namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    public class CorrectBuildingUnitDeregulationRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de gebouweenheid.
        /// </summary>
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
