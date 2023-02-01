namespace BuildingRegistry.Api.Legacy.Abstractions.Building.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
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
                Published = building.RecordCreatedAt.ToBelgianDateTimeOffset(),
                LastUpdated = building.LastChangedOn.ToBelgianDateTimeOffset(),
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

                //item.AddLink(
                //    new SyndicationLink(
                //        new Uri(string.Format(responseOptions.Value.GebouwDetailUrl, building.PersistentLocalId)),
                //        AtomLinkTypes.Self));

                //item.AddLink(
                //    new SyndicationLink(
                //            new Uri(string.Format($"{responseOptions.Value.GebouwDetailUrl}.xml", building.PersistentLocalId)),
                //            AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Xml });

                //item.AddLink(
                //    new SyndicationLink(
                //            new Uri(string.Format($"{responseOptions.Value.GebouwDetailUrl}.json", building.PersistentLocalId)),
                //            AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category1));

            item.AddCategory(
                new SyndicationCategory(category2));

            item.AddContributor(
                new SyndicationPerson(
                    building.Organisation == null ? Organisation.Unknown.ToName() : building.Organisation.Value.ToName(),
                    string.Empty,
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
                        : BuildingHelpers.GetBuildingPolygon(building.Geometry)?.XmlPolygon,
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
                                : BuildingUnitHelpers.GetBuildingUnitPoint(unit.Geometry)?.XmlPoint,
                            unit.Function.ConvertFromBuildingUnitFunction(),
                            unit.AddressIds.ToList(),
                            unit.Version.ToBelgianDateTimeOffset(),
                            unit.IsComplete,
                            unit.HasDeviation))
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
        public string BuildingId { get; set; }

        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public GebouwIdentificator Identificator { get; set; }

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
            string buildingId,
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
            Identificator = new GebouwIdentificator(naamruimte, persistentLocalId?.ToString(CultureInfo.InvariantCulture), version);
            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry == null ? null : new SyndicationPolygon { XmlPolygon = geometry };
            IsComplete = isComplete;
            BuildingUnits = buildingUnits;

            Provenance = new Provenance(version, organisation, new Reason(reason));
        }
    }

    [DataContract(Name = "Gebouweenheid", Namespace = "")]
    public class BuildingUnitSyndicationContent
    {
        /// <summary>
        /// De technische id van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string BuildingUnitId { get; set; }

        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public GebouweenheidIdentificator Identificator { get; set; }

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
        [DataMember(Name = "AdressenIds", Order = 7)]
        public List<string> Addresses { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gebouweenheid afwijking.
        /// </summary>
        [DataMember(Name = "AfwijkingVastgesteld", Order = 8)]
        public bool HasDeviation { get; set; }

        public BuildingUnitSyndicationContent(
            string buildingUnitId,
            string naamruimte,
            int? persistentLocalId,
            GebouweenheidStatus? status,
            PositieGeometrieMethode? geometryMethod,
            GmlPoint geometry,
            GebouweenheidFunctie? function,
            List<string> addresses,
            DateTimeOffset version,
            bool isComplete,
            bool hasDeviation)
        {
            BuildingUnitId = buildingUnitId;
            Identificator = new GebouweenheidIdentificator(naamruimte, persistentLocalId?.ToString(CultureInfo.InvariantCulture), version);
            Status = status;
            GeometryMethod = geometryMethod;
            Geometry = geometry == null ? null : new SyndicationPoint { XmlPoint = geometry };
            Function = function;
            Addresses = addresses;
            IsComplete = isComplete;
            HasDeviation = hasDeviation;
        }
    }

    public class BuildingSyndicationResponseExamples : IExamplesProvider<XmlElement>
    {
        private const string RawXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.vlaanderen.be/v1/feeds/gebouwen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'gebouwen' en 'gebouweenheden'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resources 'gebouwen' en 'gebouweenheden'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.2.23.2"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-09-18T06:25:34Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/gebouwen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/gebouwen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/gebouwen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/gebouwen?from=3&amp;limit=100&amp;embed=event,object"" rel=""next"" />
    <entry>
        <id>0</id>
        <title>BuildingWasRegistered-0</title>
        <updated>2011-05-18T19:59:07+02:00</updated>
        <published>2011-05-18T19:59:07+02:00</published>
        <author>
            <name>Gemeente</name>
        </author>
        <category term=""gebouwen"" />
        <category term=""gebouweenheden"" />
        <content>
            <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><BuildingWasRegistered><BuildingId>b9a05759-f1aa-5d51-a47d-3adaf62a8b8c</BuildingId><Provenance><Timestamp>2011-05-18T17:59:07Z</Timestamp><Organisation>Municipality</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </BuildingWasRegistered>
  </Event><Object><Id>b9a05759-f1aa-5d51-a47d-3adaf62a8b8c</Id><Identificator><Id>https://data.vlaanderen.be/id/gebouw/</Id><Naamruimte>https://data.vlaanderen.be/id/gebouw</Naamruimte><ObjectId /><VersieId>2011-05-18T19:59:07+02:00</VersieId></Identificator><GebouwStatus i:nil=""true"" /><GeometrieMethode i:nil=""true"" /><GeometriePolygoon i:nil=""true"" /><Gebouweenheden /><IsCompleet>false</IsCompleet><AfwijkingVastgesteld>false</AfwijkingVastgesteld><Creatie><Tijdstip>2011-05-18T19:59:07+02:00</Tijdstip><Organisatie>Gemeente</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
<entry>
    <id>2</id>
    <title>BuildingWasRealized-2</title>
    <updated>2011-05-19T10:51:09+02:00</updated>
    <published>2011-05-18T19:59:07+02:00</published>
    <author>
        <name>Agentschap voor Geografische Informatie Vlaanderen</name>
    </author>
    <category term=""gebouwen"" />
    <category term=""gebouweenheden"" />
    <content>
        <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><BuildingWasRealized><BuildingId>b9a05759-f1aa-5d51-a47d-3adaf62a8b8c</BuildingId><Provenance><Timestamp>2011-05-19T08:51:09Z</Timestamp><Organisation>Agiv</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </BuildingWasRealized>
  </Event><Object><Id>b9a05759-f1aa-5d51-a47d-3adaf62a8b8c</Id><Identificator><Id>https://data.vlaanderen.be/id/gebouw/</Id><Naamruimte>https://data.vlaanderen.be/id/gebouw</Naamruimte><ObjectId /><VersieId>2011-05-19T10:51:09+02:00</VersieId></Identificator><GebouwStatus>Gerealiseerd</GebouwStatus><GeometrieMethode i:nil=""true"" /><GeometriePolygoon i:nil=""true"" /><Gebouweenheden /><IsCompleet>false</IsCompleet><Creatie><Tijdstip>2011-05-19T10:51:09+02:00</Tijdstip><Organisatie>Agentschap voor Geografische Informatie Vlaanderen</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
</feed>";

        public XmlElement GetExamples()
        {
            var example = new XmlDocument();
            example.LoadXml(RawXml);
            return example.DocumentElement;
        }
    }
}
