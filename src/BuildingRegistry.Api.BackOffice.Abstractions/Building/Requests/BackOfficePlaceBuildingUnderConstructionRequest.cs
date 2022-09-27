namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "GebouwInAanbouw", Namespace = "")]
    public class BackOfficePlaceBuildingUnderConstructionRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
