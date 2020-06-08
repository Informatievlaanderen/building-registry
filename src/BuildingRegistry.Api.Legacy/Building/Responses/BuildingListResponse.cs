namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    [DataContract(Name = "GebouwCollectie", Namespace = "")]
    public class BuildingListResponse
    {
        /// <summary>
        /// Collectie van gebouwen
        /// </summary>
        [DataMember(Name = "Gebouwen", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouwCollectieItem> Gebouwen { get; set; }

        /// <summary>
        /// Het totaal aantal gebouwen die overeenkomen met de vraag.
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

    [DataContract(Name = "Gebouw", Namespace = "")]
    public class GebouwCollectieItem
    {
        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkele gebouw leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouw.
        /// </summary>
        [DataMember(Name = "GebouwStatus", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwStatus Status { get; set; }

        public GebouwCollectieItem(
            int id,
            string naamruimte,
            string detail,
            GebouwStatus status,
            DateTimeOffset version)
        {
            Identificator = new GebouwIdentificator(naamruimte, id.ToString(), version);
            Status = status;
            Detail = new Uri(string.Format(detail, id));
        }
    }

    public class BuildingListResponseExamples : IExamplesProvider<BuildingListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingListResponse GetExamples()
            => new BuildingListResponse
            {
                Gebouwen = new List<GebouwCollectieItem>
                {
                    new GebouwCollectieItem(6, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.Gehistoreerd, DateTimeOffset.Now),
                    new GebouwCollectieItem(7, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.Gepland, DateTimeOffset.Now.AddHours(1)),
                    new GebouwCollectieItem(8, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.Gerealiseerd, DateTimeOffset.Now.AddDays(1)),
                    new GebouwCollectieItem(9, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.InAanbouw, DateTimeOffset.Now.AddHours(9)),
                    new GebouwCollectieItem(10, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.NietGerealiseerd, DateTimeOffset.Now.AddDays(2))
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouwVolgendeUrl, "5", "10"))
            };
    }
}
