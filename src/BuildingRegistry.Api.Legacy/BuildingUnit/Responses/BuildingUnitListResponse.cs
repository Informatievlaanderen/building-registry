namespace BuildingRegistry.Api.Legacy.BuildingUnit.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

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

        public GebouweenheidCollectieItem(int id,
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

    public class BuildingUnitListResponseExamples : IExamplesProvider<BuildingUnitListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitListResponse GetExamples()
            => new BuildingUnitListResponse
            {
                Gebouweenheden = new List<GebouweenheidCollectieItem>
                {
                    new GebouweenheidCollectieItem(6, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gepland, DateTimeOffset.Now.ToExampleOffset()),
                    new GebouweenheidCollectieItem(7, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gerealiseerd, DateTimeOffset.Now.AddHours(1).ToExampleOffset()),
                    new GebouweenheidCollectieItem(8, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.NietGerealiseerd, DateTimeOffset.Now.AddDays(1).ToExampleOffset()),
                    new GebouweenheidCollectieItem(9, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gehistoreerd, DateTimeOffset.Now.AddHours(9).ToExampleOffset()),
                    new GebouweenheidCollectieItem(10, _responseOptions.GebouweenheidNaamruimte, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatus.Gerealiseerd, DateTimeOffset.Now.AddDays(2).ToExampleOffset())
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouweenheidVolgendeUrl, "5", "10"))
            };
    }
}
