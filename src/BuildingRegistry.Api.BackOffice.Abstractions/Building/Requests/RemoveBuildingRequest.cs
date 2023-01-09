namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    public sealed class RemoveBuildingRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het gebouw.
        /// </summary>
        public int PersistentLocalId { get; set; }
    }
}
