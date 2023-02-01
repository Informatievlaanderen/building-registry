namespace BuildingRegistry.Api.Oslo.Building.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouwCollectie", Namespace = "")]
    public class BuildingListOsloResponse
    {
        /// <summary>
        /// De linked-data context van het gebouw.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// Collectie van gebouwen
        /// </summary>
        [DataMember(Name = "Gebouwen", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouwCollectieItemOslo> Gebouwen { get; set; }

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
    public class GebouwCollectieItemOslo
    {
        /// <summary>
        /// Het linked-data type van het gebouw.
        /// </summary>
        [DataMember(Name = "@type", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Gebouw";

        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van het gebouw weergeeft.
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

        public GebouwCollectieItemOslo(
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

    public class BuildingListResponseOsloExamples : IExamplesProvider<BuildingListOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingListResponseOsloExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingListOsloResponse GetExamples()
            => new BuildingListOsloResponse
            {
                Gebouwen = new List<GebouwCollectieItemOslo>
                {
                    new GebouwCollectieItemOslo(6, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.Gehistoreerd, DateTimeOffset.Now.ToExampleOffset()),
                    new GebouwCollectieItemOslo(7, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.Gepland, DateTimeOffset.Now.AddHours(1).ToExampleOffset()),
                    new GebouwCollectieItemOslo(8, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.Gerealiseerd, DateTimeOffset.Now.AddDays(1).ToExampleOffset()),
                    new GebouwCollectieItemOslo(9, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.InAanbouw, DateTimeOffset.Now.AddHours(9).ToExampleOffset()),
                    new GebouwCollectieItemOslo(10, _responseOptions.GebouwNaamruimte, _responseOptions.GebouwDetailUrl, GebouwStatus.NietGerealiseerd, DateTimeOffset.Now.AddDays(2).ToExampleOffset())
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouwVolgendeUrl, "5", "10")),
                Context = _responseOptions.ContextUrlList
            };
    }
}
