namespace BuildingRegistry.Api.Legacy.BuildingUnit.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    [DataContract(Name = "GebouweenheidCollectie", Namespace = "")]
    public class BuildingUnitListResponse
    {
        /// <summary>
        /// Collectie van gebouweenheden.
        /// </summary>
        [DataMember(Name = "Gebouweenheden")]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouweenheidCollectieItem> Gebouweenheden { get; set; }

        /// <summary>
        /// Het totaal aantal gebouweneenheden die overeenkomen met de vraag.
        /// </summary>
        //[DataMember(Name = "TotaalAantal", Order = 2)]
        //[JsonProperty(Required = Required.DisallowNull)]
        //public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "Gebouweenheid", Namespace = "")]
    public class GebouweenheidCollectieItem
    {
        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkele gebouweenheid leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        public GebouweenheidCollectieItem(int id,
            string naamruimte,
            string detail,
            DateTimeOffset version)
        {
            Identificator = new GebouweenheidIdentificator(naamruimte, id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
        }
    }

    public class BuildingUnitListResponseExamples : IExamplesProvider<BuildingUnitListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitListResponse GetExamples()
            => new BuildingUnitListResponse
            {
                Gebouweenheden = new List<GebouweenheidCollectieItem>
                {
                    new GebouweenheidCollectieItem(6, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, DateTimeOffset.Now),
                    new GebouweenheidCollectieItem(7, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, DateTimeOffset.Now.AddHours(1)),
                    new GebouweenheidCollectieItem(8, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, DateTimeOffset.Now.AddDays(1)),
                    new GebouweenheidCollectieItem(9, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, DateTimeOffset.Now.AddHours(9)),
                    new GebouweenheidCollectieItem(10, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, DateTimeOffset.Now.AddDays(2))
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouweenheidVolgendeUrl, "5", "10"))
            };
    }
}
