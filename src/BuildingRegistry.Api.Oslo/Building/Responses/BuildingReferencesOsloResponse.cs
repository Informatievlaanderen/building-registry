namespace BuildingRegistry.Api.Oslo.Building.Responses
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouwReferenties", Namespace = "")]
    public class BuildingReferencesOsloResponse
    {
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        [DataMember(Name = "Crab", Order = 2)]
        public CrabReferences Crab { get; set; }

        public BuildingReferencesOsloResponse(
            int objectId,
            string @namespace,
            DateTimeOffset version,
            CrabReferences crab)
        {
            Identificator = new GebouwIdentificator(@namespace, objectId.ToString(), version);
            Crab = crab;
        }
    }

    /// <summary> De referenties van CRAB.</summary>
    [DataContract(Name = "CrabReferenties", Namespace = "")]
    public class CrabReferences
    {
        /// <summary>
        /// De CRAB TerreinObjectId.
        /// </summary>
        [DataMember(Name = "TerreinObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public int TerrainObjectId { get; set; }

        /// <summary>
        /// Het CRAB IdentificatorTerreinObject<br/>
        /// (= OIDN van de corresponderende GRB-gebouwgeometrie, <br/>
        ///  enige identificator waarmee in Lara op gebouw kan gezocht worden.).
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

    public class BuildingReferencesOsloResponseExamples : IExamplesProvider<BuildingReferencesOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingReferencesOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingReferencesOsloResponse GetExamples()
            => new BuildingReferencesOsloResponse(
                45127,
                _responseOptions.GebouwNaamruimte,
                DateTimeOffset.Now.ToExampleOffset(),
                new CrabReferences(
                    15784,
                    "787748"));
    }
}
