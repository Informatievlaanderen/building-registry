namespace BuildingRegistry.Api.Oslo.Building.V3.List
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class BuildingListOsloV3Response
    {
        /// <summary>
        /// De linked-data context van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// Het linked-data type van de gebouwen envelop.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "GebouwenEnvelop";

        /// <summary>
        /// Collectie van gebouwen
        /// </summary>
        [JsonProperty(PropertyName = "data", Order = 2, Required = Required.DisallowNull)]
        public List<GebouwCollectieItemOsloV3> Gebouwen { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [JsonProperty(PropertyName = "volgende", Order = 99, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri? Volgende { get; set; }

        [JsonIgnore]
        public SortingHeader Sorting { get; set; }

        [JsonIgnore]
        public PaginationInfo Pagination { get; set; }
    }

    public class GebouwCollectieItemOsloV3
    {
        /// <summary>
        /// Het linked-data type van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Gebouw";

        /// <summary>
        /// De unieke en persistente identificator van het gebouw (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "identificator", Order = 2, Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van het gebouw weergeeft.
        /// </summary>
        [JsonProperty(PropertyName = "detail", Order = 3, Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 4, Required = Required.DisallowNull)]
        public GebouwStatus Status { get; set; }

        public GebouwCollectieItemOsloV3(
            int persistentLocalId,
            string detail,
            GebouwStatusValue status,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.Gebouw.ToPuri(persistentLocalId.ToString());
            Identificator = new GebouwIdentificator(persistentLocalId.ToString(), version);
            Status = new GebouwStatus(status);
            Detail = new Uri(string.Format(detail, persistentLocalId));
        }
    }

    public class BuildingListResponseOsloExamples : IExamplesProvider<BuildingListOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public BuildingListResponseOsloExamples(IOptions<ResponseOptionsV3> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingListOsloV3Response GetExamples()
            => new BuildingListOsloV3Response
            {
                Gebouwen = new List<GebouwCollectieItemOsloV3>
                {
                    new GebouwCollectieItemOsloV3(6, _responseOptions.GebouwDetailUrl, GebouwStatusValue.Gehistoreerd, DateTimeOffset.Now.ToExampleOffset()),
                    new GebouwCollectieItemOsloV3(7, _responseOptions.GebouwDetailUrl, GebouwStatusValue.Gepland, DateTimeOffset.Now.AddHours(1).ToExampleOffset()),
                    new GebouwCollectieItemOsloV3(8, _responseOptions.GebouwDetailUrl, GebouwStatusValue.Gerealiseerd, DateTimeOffset.Now.AddDays(1).ToExampleOffset()),
                    new GebouwCollectieItemOsloV3(9, _responseOptions.GebouwDetailUrl, GebouwStatusValue.InAanbouw, DateTimeOffset.Now.AddHours(9).ToExampleOffset()),
                    new GebouwCollectieItemOsloV3(10, _responseOptions.GebouwDetailUrl, GebouwStatusValue.NietGerealiseerd, DateTimeOffset.Now.AddDays(2).ToExampleOffset())
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouwVolgendeUrl, "5", "10")),
                Context = _responseOptions.ContextUrlList
            };
    }
}
