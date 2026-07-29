namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.List
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class BuildingUnitListOsloV3Response
    {
        /// <summary>
        /// De linked-data context van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// Het linked-data type van de gebouweenheid envelop.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "GebouweenhedenEnvelop";

        /// <summary>
        /// Collectie van gebouweenheden.
        /// </summary>
        [JsonProperty(PropertyName = "data", Order = 2, Required = Required.DisallowNull)]
        public List<GebouweenheidCollectieItemOsloV3> Gebouweenheden { get; set; }

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

    public class GebouweenheidCollectieItemOsloV3
    {
        /// <summary>
        /// Het linked-data type van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Gebouweenheid";

        /// <summary>
        /// De unieke en persistente identificator van de gebouweenheid (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set;}

        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "identificator", Order = 2, Required = Required.DisallowNull)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van de gebouweenheid weergeeft.
        /// </summary>
        [JsonProperty(PropertyName = "detail", Order = 3, Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 4, Required = Required.DisallowNull)]
        public GebouweenheidStatus Status { get; set; }

        public GebouweenheidCollectieItemOsloV3(int id,
            string detail,
            GebouweenheidStatusValue status,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.Gebouweenheid.ToPuri(id.ToString());
            Identificator = new GebouweenheidIdentificator(id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Status = new GebouweenheidStatus(status);
        }
    }

    public class BuildingUnitListOsloResponseExamples : IExamplesProvider<BuildingUnitListOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public BuildingUnitListOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitListOsloV3Response GetExamples()
            => new BuildingUnitListOsloV3Response
            {
                Gebouweenheden = new List<GebouweenheidCollectieItemOsloV3>
                {
                    new GebouweenheidCollectieItemOsloV3(6, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatusValue.Gepland, DateTimeOffset.Now.ToExampleOffset()),
                    new GebouweenheidCollectieItemOsloV3(7, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatusValue.Gerealiseerd, DateTimeOffset.Now.AddHours(1).ToExampleOffset()),
                    new GebouweenheidCollectieItemOsloV3(8, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatusValue.NietGerealiseerd, DateTimeOffset.Now.AddDays(1).ToExampleOffset()),
                    new GebouweenheidCollectieItemOsloV3(9, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatusValue.Gehistoreerd, DateTimeOffset.Now.AddHours(9).ToExampleOffset()),
                    new GebouweenheidCollectieItemOsloV3(10, _responseOptions.GebouweenheidDetailUrl, GebouweenheidStatusValue.Gerealiseerd, DateTimeOffset.Now.AddDays(2).ToExampleOffset())
                },
                Volgende = new Uri(string.Format(_responseOptions.GebouweenheidVolgendeUrl, "5", "10")),
                Context = _responseOptions.ContextUrlUnitList
            };
    }
}
