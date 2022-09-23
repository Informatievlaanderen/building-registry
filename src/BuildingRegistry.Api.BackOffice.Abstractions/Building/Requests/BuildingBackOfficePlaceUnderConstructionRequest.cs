namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "GebouwInAanbouw", Namespace = "")]
    public sealed class BuildingBackOfficePlaceUnderConstructionRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
