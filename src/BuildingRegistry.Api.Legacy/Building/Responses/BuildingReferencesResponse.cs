namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouwReferenties", Namespace = "")]
    public class BuildingReferencesResponse
    {
        /// <summary>
        /// De referenties van CRAB.
        /// </summary>
        [DataMember(Name = "Crab", Order = 1)]
        public CrabReferences Crab { get; set; }

        public BuildingReferencesResponse(CrabReferences crab)
        {
            Crab = crab;
        }
    }

    [DataContract(Name = "CrabReferenties", Namespace = "")]
    public class CrabReferences
    {
        /// <summary>
        /// Het CRAB TerreinObjectId.
        /// </summary>
        [DataMember(Name = "TerreinObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public int TerrainObjectId { get; set; }

        /// <summary>
        /// De CRAB IdentificatorTerreinObject.
        /// </summary>
        [DataMember(Name = "IdentificatorTerreinObject", Order = 2)]
        public string IdentifierTerrainObject { get; set; }

        public CrabReferences(
            int terrainObjectId,
            string identifierTerrainObject)
        {
            TerrainObjectId = terrainObjectId;
            IdentifierTerrainObject = identifierTerrainObject;
        }
    }

    public class BuildingReferencesResponseExamples : IExamplesProvider<BuildingReferencesResponse>
    {
        public BuildingReferencesResponse GetExamples()
            => new BuildingReferencesResponse(
                new CrabReferences(
                    15784,
                    "787748"));
    }
}
