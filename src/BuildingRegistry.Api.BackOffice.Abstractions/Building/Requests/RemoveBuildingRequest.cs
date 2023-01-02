namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "VerwijderGebouw", Namespace = "")]
    public sealed class RemoveBuildingRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het gebouw.
        /// </summary>
        public int PersistentLocalId { get; set; }
    }
}
