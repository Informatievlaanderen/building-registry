namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    public sealed class CorrectBuildingUnitRegularizationRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de gebouweenheid.
        /// </summary>
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
