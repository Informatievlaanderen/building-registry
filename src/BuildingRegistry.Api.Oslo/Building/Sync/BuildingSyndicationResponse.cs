namespace BuildingRegistry.Api.Oslo.Building.Sync
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
    <id>https://api.basisregisters.vlaanderen.be/v2/feeds/gebouwen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'gebouwen' en 'gebouweenheden'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resources 'gebouwen' en 'gebouweenheden'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.2.23.2"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-09-18T06:25:34Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/gebouwen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/gebouwen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/gebouwen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/gebouwen?from=3&amp;limit=100&amp;embed=event,object"" rel=""next"" />
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
<entry>
  <id>254204954</id>
  <title>BuildingWasMigrated-254204954</title>
  <updated>2023-11-02T07:24:43+01:00</updated>
  <published>2023-11-02T07:24:43+01:00</published>
  <link href=""https://data.vlaanderen.be/id/gebouw/6355606"" rel=""related"" />
  <author>
    <name>Digitaal Vlaanderen</name>
  </author>
  <category term=""gebouwen"" />
  <category term=""gebouweenheden"" />
  <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
    <Event>
      <BuildingWasMigrated>
        <BuildingId>f974f3ba-abac-5162-8ed9-6a1154dc0c08</BuildingId>
        <BuildingPersistentLocalId>6355606</BuildingPersistentLocalId>
        <BuildingPersistentLocalIdAssignmentDate>2017-09-30T12:09:07Z</BuildingPersistentLocalIdAssignmentDate>
        <BuildingStatus>Realized</BuildingStatus>
        <GeometryMethod>MeasuredByGrb</GeometryMethod>
        <ExtendedWkbGeometry>01030000208A7A0000010000000900000000A0C7B467A0014180F1AA0069A00841003E7A4F56A0014100AA59997AA008410095297239A001418086D5515FA00841007E53E441A001410086F40D56A0084100E3E57545A0014100C8A52352A00841001DC97148A0014180BE92DD4EA0084100E9863285A00141801E3A360CA008410038C745A6A001410011988029A0084100A0C7B467A0014180F1AA0069A00841</ExtendedWkbGeometry>
        <IsRemoved>false</IsRemoved>
        <BuildingUnits>
          <BuildingUnits_0>
            <BuildingUnitId>d0ec85d4-d4ba-50e3-80f0-35f5fd44383f</BuildingUnitId>
            <BuildingUnitPersistentLocalId>6356866</BuildingUnitPersistentLocalId>
            <Function>Unknown</Function>
            <Status>Realized</Status>
            <AddressPersistentLocalIds>
              <AddressPersistentLocalIds_0>2434522</AddressPersistentLocalIds_0>
            </AddressPersistentLocalIds>
            <GeometryMethod>AppointedByAdministrator</GeometryMethod>
            <ExtendedWkbGeometry>01010000208A7A00005C8FC2F588A001417B14AE471FA00841</ExtendedWkbGeometry>
            <IsRemoved>false</IsRemoved>
          </BuildingUnits_0>
        </BuildingUnits>
        <Provenance>
          <Timestamp>2023-11-02T06:24:43Z</Timestamp>
          <Organisation>DigitaalVlaanderen</Organisation>
          <Reason>Migrate Building aggregate.</Reason>
        </Provenance>
      </BuildingWasMigrated>
    </Event>
    <Object>
      <Id>6355606</Id>
      <Identificator>
        <Id>https://data.vlaanderen.be/id/gebouw/6355606</Id>
        <Naamruimte>https://data.vlaanderen.be/id/gebouw</Naamruimte>
        <ObjectId>6355606</ObjectId>
        <VersieId>2023-11-02T07:24:43+01:00</VersieId>
      </Identificator>
      <GebouwStatus>Gerealiseerd</GebouwStatus>
      <GeometrieMethode>IngemetenGRB</GeometrieMethode>
      <GeometriePolygoon>
        <polygon>
          <exterior>
            <LinearRing>
              <posList>144396.96327137947 201741.12532604858 144394.78880737722 201743.32487805188 144391.18074337393 201739.91495804861 144392.23648737371 201738.75681404769 144392.68256738037 201738.26740604639 144393.05555937439 201737.85819004849 144400.64967138320 201729.52647804096 144404.78407138586 201733.18779004365 144396.96327137947 201741.12532604858</posList>
            </LinearRing>
          </exterior>
        </polygon>
      </GeometriePolygoon>
      <Gebouweenheden>
        <Gebouweenheid>
          <Id>6356866</Id>
          <Identificator>
            <Id>https://data.vlaanderen.be/id/gebouweenheid/6356866</Id>
            <Naamruimte>https://data.vlaanderen.be/id/gebouweenheid</Naamruimte>
            <ObjectId>6356866</ObjectId>
            <VersieId>2023-11-02T07:24:43+01:00</VersieId>
          </Identificator>
          <GebouweenheidStatus>Gerealiseerd</GebouweenheidStatus>
          <PositieGeometrieMethode>AangeduidDoorBeheerder</PositieGeometrieMethode>
          <GeometriePunt>
            <point>
              <pos>144401.12 201731.91</pos>
            </point>
          </GeometriePunt>
          <Functie>NietGekend</Functie>
          <AdressenIds xmlns:d5p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">
            <d5p1:string>2434522</d5p1:string>
          </AdressenIds>
          <AfwijkingVastgesteld>false</AfwijkingVastgesteld>
          <IsCompleet>true</IsCompleet>
        </Gebouweenheid>
      </Gebouweenheden>
      <IsCompleet>true</IsCompleet>
      <Creatie>
        <Tijdstip>2023-11-02T07:24:43+01:00</Tijdstip>
        <Organisatie>Digitaal Vlaanderen</Organisatie>
        <Reden>Migrate Building aggregate.</Reden>
      </Creatie>
    </Object>
  </Content>]]></content>
</entry>
<entry>
  <id>267204947</id>
  <title>BuildingUnitWasPlannedV2-267204947</title>
  <updated>2024-11-14T15:22:57+01:00</updated>
  <published>2023-11-04T02:05:53+01:00</published>
  <link href=""https://data.vlaanderen.be/id/gebouw/31255209"" rel=""related"" />
  <author>
    <name>Andere</name>
  </author>
  <category term=""gebouwen"" />
  <category term=""gebouweenheden"" />
  <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
    <Event>
      <BuildingUnitWasPlannedV2>
        <BuildingPersistentLocalId>31255209</BuildingPersistentLocalId>
        <BuildingUnitPersistentLocalId>31710876</BuildingUnitPersistentLocalId>
        <GeometryMethod>DerivedFromObject</GeometryMethod>
        <ExtendedWkbGeometry>01010000208A7A000022DDEACFBB97FE40BF69B43057150641</ExtendedWkbGeometry>
        <Function>Unknown</Function>
        <HasDeviation>false</HasDeviation>
        <Provenance>
          <Timestamp>2024-11-14T14:22:57Z</Timestamp>
          <Organisation>Other</Organisation>
          <Reason>
          </Reason>
        </Provenance>
      </BuildingUnitWasPlannedV2>
    </Event>
    <Object>
      <Id>31255209</Id>
      <Identificator>
        <Id>https://data.vlaanderen.be/id/gebouw/31255209</Id>
        <Naamruimte>https://data.vlaanderen.be/id/gebouw</Naamruimte>
        <ObjectId>31255209</ObjectId>
        <VersieId>2024-11-14T15:22:57+01:00</VersieId>
      </Identificator>
      <GebouwStatus>Gerealiseerd</GebouwStatus>
      <GeometrieMethode>IngemetenGRB</GeometrieMethode>
      <GeometriePolygoon>
        <polygon>
          <exterior>
            <LinearRing>
              <posList>125301.87781818211 180907.61657565087 125304.34297018498 180902.05222364515 125313.58802618831 180906.14803164825 125311.15698618442 180911.68403165415 125311.12178618461 180911.76390365139 125301.87781818211 180907.61657565087</posList>
            </LinearRing>
          </exterior>
        </polygon>
      </GeometriePolygoon>
      <Gebouweenheden>
        <Gebouweenheid>
          <Id>31710876</Id>
          <Identificator>
            <Id>https://data.vlaanderen.be/id/gebouweenheid/31710876</Id>
            <Naamruimte>https://data.vlaanderen.be/id/gebouweenheid</Naamruimte>
            <ObjectId>31710876</ObjectId>
            <VersieId>2024-11-14T15:22:57+01:00</VersieId>
          </Identificator>
          <GebouweenheidStatus>Gepland</GebouweenheidStatus>
          <PositieGeometrieMethode>AfgeleidVanObject</PositieGeometrieMethode>
          <GeometriePunt>
            <point>
              <pos>125307.74 180906.90</pos>
            </point>
          </GeometriePunt>
          <Functie>NietGekend</Functie>
          <AdressenIds xmlns:d5p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" />
          <AfwijkingVastgesteld>false</AfwijkingVastgesteld>
          <IsCompleet>true</IsCompleet>
        </Gebouweenheid>
      </Gebouweenheden>
      <IsCompleet>true</IsCompleet>
      <Creatie>
        <Tijdstip>2024-11-14T15:22:57+01:00</Tijdstip>
        <Organisatie>Andere</Organisatie>
        <Reden />
      </Creatie>
    </Object>
  </Content>]]></content>
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
