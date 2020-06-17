namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CrabGebouwenCollectie", Namespace = "")]
    public class BuildingCrabMappingResponse
    {
        /// <summary>
        /// Collectie van Crab Gebouwen
        /// </summary>
        [DataMember(Name = "CrabGebouwen", Order = 1)]
        [XmlArrayItem(ElementName = "CrabGebouw")]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<BuildingCrabMappingItem> CrabGebouwen { get; set; }
    }

    [DataContract(Name = "CrabGebouw", Namespace = "")]
    public class BuildingCrabMappingItem
    {
        /// <summary>
        /// De TerreinObjectId gekend in CRAB.
        /// </summary>
        [DataMember(Name = "TerreinObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public int TerrainObjectId { get; set; }

        /// <summary>
        /// De IdentificatorTerreinObject gekend in CRAB.
        /// </summary>
        [DataMember(Name = "IdentificatorTerreinObject", Order = 2)]
        public string IdentifierTerrainObject { get; set; }

        /// <summary>
        /// Het ObjectId van het gebouw in gebouwenregister.
        /// </summary>
        [DataMember(Name = "GebouwObjectId", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public int PersistentLocalId { get; set; }

        public BuildingCrabMappingItem(
            int terrainObjectId,
            string identifierTerrainObject,
            int persistentLocalId)
        {
            TerrainObjectId = terrainObjectId;
            IdentifierTerrainObject = identifierTerrainObject;
            PersistentLocalId = persistentLocalId;
        }
    }

    public class BuildingCrabMappingResponseExamples : IExamplesProvider<BuildingCrabMappingResponse>
    {
        public BuildingCrabMappingResponse GetExamples()
            => new BuildingCrabMappingResponse
            {
                CrabGebouwen = new List<BuildingCrabMappingItem>
                {
                    new BuildingCrabMappingItem(6, "4897515", 15267),
                    new BuildingCrabMappingItem(7, string.Empty, 987415),
                    new BuildingCrabMappingItem(8, "7714587", 4845125),
                },
            };
    }
}
