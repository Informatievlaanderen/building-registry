namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingUnit;
    using Converters;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class BuildingSyndicationResponse
    {
        public static async Task WriteBuilding(
            this ISyndicationFeedWriter writer,
            IOptions<ResponseOptions> responseOptions,
            AtomFormatter formatter,
            string category1,
            string category2,
            BuildingSyndicationQueryResult building)
        {
            var item = new SyndicationItem
            {
                Id = building.Position.ToString(CultureInfo.InvariantCulture),
                Title = $"{building.ChangeType}-{building.Position}",
                Published = building.RecordCreatedAt.ToDateTimeOffset(),
                LastUpdated = building.LastChangedOn.ToDateTimeOffset(),
                Description = BuildDescription(
                    building,
                    responseOptions.Value.GebouwNaamruimte,
                    responseOptions.Value.GebouweenheidNaamruimte)
            };

            if (building.PersistentLocalId.HasValue)
            {
                item.AddLink(
                    new SyndicationLink(
                        new Uri($"{responseOptions.Value.GebouwNaamruimte}/{building.PersistentLocalId}"),
                        AtomLinkTypes.Related));

                item.AddLink(
                    new SyndicationLink(
                        new Uri(string.Format(responseOptions.Value.GebouwDetailUrl, building.PersistentLocalId)),
                        AtomLinkTypes.Self));

                item.AddLink(
                    new SyndicationLink(
                            new Uri(string.Format($"{responseOptions.Value.GebouwDetailUrl}.xml", building.PersistentLocalId)),
                            AtomLinkTypes.Alternate)
                    { MediaType = MediaTypeNames.Application.Xml });

                item.AddLink(
                    new SyndicationLink(
                            new Uri(string.Format($"{responseOptions.Value.GebouwDetailUrl}.json", building.PersistentLocalId)),
                            AtomLinkTypes.Alternate)
                    { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category1));

            item.AddCategory(
                new SyndicationCategory(category2));

            item.AddContributor(
                new SyndicationPerson(
                    "agentschap Informatie Vlaanderen",
                    "informatie.vlaanderen@vlaanderen.be",
                    AtomContributorTypes.Author));

            await writer.Write(new Microsoft.SyndicationFeed.SyndicationContent(formatter.CreateContent(item)));
        }

        private static string BuildDescription(
            BuildingSyndicationQueryResult building,
            string naamruimte,
            string gebouweenheidNaamruimte)
        {
            if (!building.ContainsEvent && !building.ContainsObject)
                return "No data embedded";

            var syndicationContent = new SyndicationContent();
            if (building.ContainsObject)
            {
                syndicationContent.Object = new BuildingSyndicationContent(
                    building.BuildingId,
                    naamruimte,
                    building.PersistentLocalId,
                    building.Status?.ConvertFromBuildingStatus(),
                    building.GeometryMethod?.ConvertFromBuildingGeometryMethod(),
                    building.Geometry == null
                        ? null
                        : BuildingController.GetBuildingPolygon(building.Geometry).XmlPolygon,
                    building.LastChangedOn.ToBelgianDateTimeOffset(),
                    building.IsComplete,
                    building.Organisation,
                    building.Reason,
                    building
                        .BuildingUnits
                        .Select(unit => new BuildingUnitSyndicationContent(
                            unit.BuildingUnitId,
                            gebouweenheidNaamruimte,
                            unit.PersistentLocalId,
                            unit.Status?.ConvertFromBuildingUnitStatus(),
                            unit.GeometryMethod?.ConvertFromBuildingUnitGeometryMethod(),
                            unit.Geometry == null
                                ? null
                                : BuildingUnitController.GetBuildingUnitPoint(unit.Geometry)?.XmlPoint,
                            unit.Function?.ConvertFromBuildingUnitFunction(),
                            unit.AddressIds.ToList(),
                            unit.Version.ToBelgianDateTimeOffset(),
                            unit.IsComplete))
                        .ToList());
            }

            if (building.ContainsEvent)
            {
                var doc = new XmlDocument();
                doc.LoadXml(building.EventDataAsXml);
                syndicationContent.Event = doc.DocumentElement;
            }

            return syndicationContent.ToXml();
        }
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent : SyndicationContentBase
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public BuildingSyndicationContent Object { get; set; }
    }

    [DataContract(Name = "Gebouw", Namespace = "")]
    public class BuildingSyndicationContent
    {
        /// <summary>
        /// De technische id van het gebouw.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid BuildingId { get; set; }

        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De fase in het leven van het gebouw.
        /// </summary>
        [DataMember(Name = "GebouwStatus", Order = 3)]
        public GebouwStatus? Status { get; set; }

        /// <summary>
        /// De geometrie methode van het gebouw.
        /// </summary>
        [DataMember(Name = "GeometrieMethode", Order = 4)]
        public GeometrieMethode? GeometryMethod { get; set; }

        /// <summary>
        /// De geometrie van het gebouw.
        /// </summary>
        [DataMember(Name = "GeometriePolygoon", Order = 5)]
        public SyndicationPolygon Geometry { get; set; }

        /// <summary>
        /// De gebouweenheden van het gebouw.
        /// </summary>
        [DataMember(Name = "Gebouweenheden", Order = 6)]
        public List<BuildingUnitSyndicationContent> BuildingUnits { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 9)]
        public Provenance Provenance { get; set; }

        public BuildingSyndicationContent(
            Guid buildingId,
            string naamruimte,
            int? persistentLocalId,
            GebouwStatus? status,
            GeometrieMethode? geometryMethod,
            GmlPolygon geometry,
            DateTimeOffset version,
            bool isComplete,
            Organisation? organisation,
            string reason,
            List<BuildingUnitSyndicationContent> buildingUnits)
        {
            BuildingId = buildingId;
            Identificator = new Identificator(naamruimte, persistentLocalId.HasValue ? persistentLocalId.ToString() : string.Empty, version);
            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry == null ? null : new SyndicationPolygon { XmlPolygon = geometry };
            IsComplete = isComplete;
            BuildingUnits = buildingUnits;

            Provenance = new Provenance(organisation, new Reason(reason));
        }
    }

    [DataContract(Name = "Gebouweenheid", Namespace = "")]
    public class BuildingUnitSyndicationContent
    {
        /// <summary>
        /// De technische id van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid BuildingUnitId { get; set; }

        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De fase in het leven van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "GebouweenheidStatus", Order = 3)]
        public GebouweenheidStatus? Status { get; set; }

        /// <summary>
        /// De geometrie methode van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 4)]
        public PositieGeometrieMethode? GeometryMethod { get; set; }

        /// <summary>
        /// De geometrie van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "GeometriePunt", Order = 5)]
        public SyndicationPoint Geometry { get; set; }

        /// <summary>
        /// De functie van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Functie", Order = 6)]
        public GebouweenheidFunctie? Function { get; set; }

        /// <summary>
        /// De gebouweenheden van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Addressen", Order = 7)]
        public List<Guid> Addresses { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }

        public BuildingUnitSyndicationContent(
            Guid buildingUnitId,
            string naamruimte,
            int? persistentLocalId,
            GebouweenheidStatus? status,
            PositieGeometrieMethode? geometryMethod,
            GmlPoint geometry,
            GebouweenheidFunctie? function,
            List<Guid> addresses,
            DateTimeOffset version,
            bool isComplete)
        {
            BuildingUnitId = buildingUnitId;
            Identificator = new Identificator(naamruimte, persistentLocalId.HasValue ? persistentLocalId.ToString() : string.Empty, version);
            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry == null ? null : new SyndicationPoint { XmlPoint = geometry };
            Function = function;
            Addresses = addresses;
            IsComplete = isComplete;
        }
    }

    public class BuildingSyndicationResponseExamples : IExamplesProvider
    {
        private SyndicationContent ContentExample
        {
            get
            {
                var buildingId = Guid.NewGuid();
                var xmlString = $"<BuildingWasRegistered><BuildingId>{buildingId}</BuildingId><Provenance><Timestamp>2019-01-20T13:13:53Z</Timestamp><Modification>Update</Modification><Organisation>Aiv</Organisation><Plan>CentralManagementCrab</Plan></Provenance></BuildingWasRegistered>";
                var doc = new XmlDocument();
                doc.LoadXml(xmlString);
                return new SyndicationContent
                {
                    Event = doc.DocumentElement,
                    Object = new BuildingSyndicationContent(
                        buildingId,
                        _responseOptions.GebouwNaamruimte,
                        13023,
                        GebouwStatus.Gerealiseerd,
                        GeometrieMethode.IngemetenGRB,
                        new GmlPolygon
                        {
                            Exterior = new RingProperty
                            {
                                LinearRing = new LinearRing
                                {
                                    PosList =
                                        "101673.0 193520.0 101673.0 193585.0 101732.0 193585.0 101673.0 193585.0 101673.0 193520.0"
                                }
                            }
                        },
                        DateTimeOffset.Now,
                        true,
                        Organisation.Agiv,
                        Reason.CentralManagementCrab,
                        new List<BuildingUnitSyndicationContent>
                        {
                            new BuildingUnitSyndicationContent(
                                Guid.NewGuid(),
                                _responseOptions.GebouweenheidNaamruimte,
                                45871,
                                GebouweenheidStatus.Gerealiseerd,
                                PositieGeometrieMethode.AfgeleidVanObject,
                                new GmlPoint
                                {
                                    Pos = "140252.76 198794.27"
                                },
                                GebouweenheidFunctie.NietGekend,
                                new List<Guid> {Guid.NewGuid()},
                                DateTimeOffset.Now,
                                true)
                        })
                };
            }
        }

        private readonly ResponseOptions _responseOptions;

        public BuildingSyndicationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples() =>
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
  <id>https://basisregisters.vlaanderen/syndication/feed/building.atom</id>
  <title>Basisregisters Vlaanderen - Gebouwregister</title>
  <subtitle>Basisregisters Vlaanderen stelt u in staat om alles te weten te komen rond: de Belgische gemeenten; de Belgische postcodes; de Vlaamse straatnamen; de Vlaamse adressen; de Vlaamse gebouwen en gebouweenheden; de Vlaamse percelen; de Vlaamse organisaties en organen; de Vlaamse dienstverlening.</subtitle>
  <generator uri=""https://basisregisters.vlaanderen"" version=""2.0.0.0"">Basisregisters Vlaanderen</generator>
  <rights>Copyright (c) 2017-2018, Informatie Vlaanderen</rights>
  <updated>2018-10-05T14:06:53Z</updated>
  <author>
    <name>agentschap Informatie Vlaanderen</name>
    <email>informatie.vlaanderen@vlaanderen.be</email>
  </author>
  <link href=""https://basisregisters.vlaanderen/syndication/feed/building.atom"" rel=""self"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/"" rel=""related"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/v1/feeds/gebouwen.atom?offset=100&limit=100"" rel=""next""/>
  <entry>
    <id>4</id>
    <title>BuildingWasRegistered-4</title>
    <updated>2018-10-04T13:12:17Z</updated>
    <published>2018-10-04T13:12:17Z</published>
    <link href=""{_responseOptions.GebouwNaamruimte}/13023"" rel=""related"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/gebouwen/13023"" rel=""self"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/gebouwen/13023.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/gebouwen/13023.json"" rel=""alternate"" type=""application/json"" />
    <author>
      <name>agentschap Informatie Vlaanderen</name>
      <email>informatie.vlaanderen@vlaanderen.be</email>
    </author>
    <category term=""https://data.vlaanderen.be/ns/gebouw"" />
    <category term=""https://data.vlaanderen.be/ns/gebouweenheid"" />
    <content><![CDATA[{ContentExample.ToXml()}]]></content>
  </entry>
</feed>";
    }
}
