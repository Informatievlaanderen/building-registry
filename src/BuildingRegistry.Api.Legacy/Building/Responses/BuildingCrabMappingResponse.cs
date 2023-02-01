namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CrabGebouwenCollectie", Namespace = "")]
    public class BuildingCrabMappingResponse
    {
        /// <summary>
        /// Collectie van CRAB gebouwen
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
        /// Het CRAB IdentificatorTerreinObject<br/>
        /// (= OIDN van de corresponderende GRB-gebouwgeometrie, enige identificator waarmee in Lara op gebouw kan gezocht worden.).
        /// </summary>
        [DataMember(Name = "IdentificatorTerreinObject", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string IdentifierTerrainObject { get; set; }

        [DataMember(Name = "Gebouw", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public CrabGrarGebouw Building { get; set; }


        public BuildingCrabMappingItem(
            int persistentLocalId,
            int terrainObjectId,
            string identifierTerrainObject)
        {
            Building = new CrabGrarGebouw(persistentLocalId);
            TerrainObjectId = terrainObjectId;
            IdentifierTerrainObject = identifierTerrainObject;
        }
    }

    /// <summary>Het gebouw in het gebouwenregister.</summary>
    [DataContract(Name = "Gebouw", Namespace = "")]
    public class CrabGrarGebouw
    {
        [DataMember(Name = "Identificator")]
        public GebouwCrabIdentificator Identifier { get; set; }

        public CrabGrarGebouw(int persistentLocalId)
        {
            Identifier = new GebouwCrabIdentificator(persistentLocalId.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>De identificator van het gebouw.</summary>
    [DataContract(Name = "Identificator", Namespace = "")]
    public class GebouwCrabIdentificator
    {
        /// <summary>
        /// De objectidentificator (enkel uniek binnen naamruimte).
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        public GebouwCrabIdentificator(string objectId)
        {
            ObjectId = objectId;
        }
    }

    public class BuildingCrabMappingResponseExamples : IExamplesProvider<BuildingCrabMappingResponse>
    {
        public BuildingCrabMappingResponse GetExamples()
            => new BuildingCrabMappingResponse
            {
                CrabGebouwen = new List<BuildingCrabMappingItem>
                {
                    new BuildingCrabMappingItem(15267, 6, "4897515"),
                    new BuildingCrabMappingItem(987415, 7, string.Empty),
                    new BuildingCrabMappingItem(4845125, 8, "7714587"),
                },
            };
    }
}
