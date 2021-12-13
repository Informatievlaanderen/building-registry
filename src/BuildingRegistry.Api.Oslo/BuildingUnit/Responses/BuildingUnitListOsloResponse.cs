namespace BuildingRegistry.Api.Oslo.BuildingUnit.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.JsonConverters;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouweenheidCollectie", Namespace = "")]
    public class BuildingUnitListOsloResponse
    {
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(PlainStringJsonConverter))]
        public object Context => @"{
	""identificator"": ""@nest"",
	""id"": ""@id"",
	""versieId"":{
		""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",        
		""@type"": ""http://www.w3.org/2001/XMLSchema#string""},
	""detail"": ""http://www.iana.org/assignments/relation/self"",	
	""gebouweenheidStatus"":{
		""@id"":""https://data.vlaanderen.be/ns/gebouw#Gebouweenheid.status"",
		""@type"":""@id"",
		""@context"":{
			""@base"":""https://data.vlaanderen.be/doc/concept/gebouweenheidsstatus/""
	  },
	  ""gebouweenheden"": ""@graph""
	}
}";

        /// <summary>
        /// Collectie van gebouweenheden.
        /// </summary>
        [DataMember(Name = "Gebouweenheden", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouweenheidCollectieItemOslo> Gebouweenheden { get; set; }

        /// <summary>
        /// Het totaal aantal gebouweneenheden die overeenkomen met de vraag.
        /// </summary>
        //[DataMember(Name = "TotaalAantal", Order = 2)]
        //[JsonProperty(Required = Required.DisallowNull)]
        //public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 2, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "Gebouweenheid", Namespace = "")]
    public class GebouweenheidCollectieItemOslo
    {
        /// <summary>
        /// Het linked-data type van de gebouw.
        /// </summary>
        [DataMember(Name = "@type", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Gebouweenheid";

        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van de gebouweenheid weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouweenheid.
        /// </summary>
        [DataMember(Name = "GebouweenheidStatus", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidStatus Status { get; set; }

        public GebouweenheidCollectieItemOslo(int id,
            string naamruimte,
            string detail,
            GebouweenheidStatus status,
            DateTimeOffset version)
        {
            Identificator = new GebouweenheidIdentificator(naamruimte, id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Status = status;
        }
    }

    public class BuildingUnitListOsloResponseExamples : IExamplesProvider<BuildingUnitListOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitListOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitListOsloResponse GetExamples()
            => new BuildingUnitListOsloResponse
            {
                Gebouweenheden = new List<GebouweenheidCollectieItemOslo>
                {
                    new GebouweenheidCollectieItemOslo(6, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gepland, DateTimeOffset.Now.ToExampleOffset()),
                    new GebouweenheidCollectieItemOslo(7, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gerealiseerd, DateTimeOffset.Now.AddHours(1).ToExampleOffset()),
                    new GebouweenheidCollectieItemOslo(8, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.NietGerealiseerd, DateTimeOffset.Now.AddDays(1).ToExampleOffset()),
                    new GebouweenheidCollectieItemOslo(9, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gehistoreerd, DateTimeOffset.Now.AddHours(9).ToExampleOffset()),
                    new GebouweenheidCollectieItemOslo(10, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gerealiseerd, DateTimeOffset.Now.AddDays(2).ToExampleOffset())
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouweenheidVolgendeUrl, "5", "10"))
            };
    }
}
