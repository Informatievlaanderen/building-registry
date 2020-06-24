namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouwReferenties", Namespace = "")]
    public class BuildingReferencesResponse
    {
        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// De referenties van CRAB.
        /// </summary>
        [DataMember(Name = "Crab", Order = 2)]
        public CrabReferences Crab { get; set; }

        public BuildingReferencesResponse(
            int objectId,
            string @namespace,
            DateTimeOffset version,
            CrabReferences crab)
        {
            Identificator = new GebouwIdentificator(@namespace, objectId.ToString(), version);
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
        /// (= OIDN van de corresponderende GRB-gebouwgeometrie)
        /// (= enige identificator waarmee in Lara op gebouw kan gezocht worden)
        /// </summary>
        [DataMember(Name = "IdentificatorTerreinObject", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
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
        private readonly ResponseOptions _responseOptions;

        public BuildingReferencesResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingReferencesResponse GetExamples()
            => new BuildingReferencesResponse(
                45127,
                _responseOptions.GebouwNaamruimte,
                DateTimeOffset.Now,
                new CrabReferences(
                    15784,
                    "787748"));
    }
}
