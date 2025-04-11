namespace BuildingRegistry.Producer.Ldes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Building;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using LinearRing = NetTopologySuite.Geometries.LinearRing;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    public class BuildingLdes
    {
        private static readonly JObject Context = JObject.Parse(@"
{
  ""@version"": 1.1,
  ""@base"": ""https://basisregisters.vlaanderen.be/implementatiemodel/gebouwenregister"",
  ""@vocab"": ""#"",
  ""identificator"": ""@nest"",
  ""id"": ""@id"",
  ""versieId"": {
    ""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
  },
  ""naamruimte"": {
    ""@id"": ""https://data.vlaanderen.be/ns/generiek#naamruimte"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
  },
  ""objectId"": {
    ""@id"": ""https://data.vlaanderen.be/ns/generiek#lokaleIdentificator"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
  },
  ""gebouwPolygoon"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouw.geometrie"",
    ""@type"": ""@id"",
    ""@context"": {
      ""geometrieMethode"": {
        ""@id"": ""https://data.vlaanderen.be/ns/gebouw#methode"",
        ""@type"": ""@id"",
        ""@context"": {
          ""@base"": ""https://data.vlaanderen.be/doc/concept/2Dgeometriemethode/""
        }
      },
      ""geometrie"": {
        ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouw.geometrie"",
        ""@context"": {
          ""gml"": {
            ""@id"": ""http://www.opengis.net/ont/geosparql#asGML"",
            ""@type"": ""http://www.opengis.net/ont/geosparql#gmlLiteral""
          },
          ""type"": ""@type"",
          ""@vocab"": ""http://www.opengis.net/ont/sf#""
        }
      }
    }
  },
  ""gebouwStatus"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouw.status"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/doc/concept/gebouwstatus/""
    }
  },
  ""gebouweenheden"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#bestaatUit"",
    ""@container"": ""@set"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/id/gebouweenheid/""
    }
  }
}");

        [JsonProperty("@context", Order = 0)]
        public JObject LdContext => Context;

        [JsonProperty("@type", Order = 1)]
        public string Type => "Gebouw";

        [JsonProperty("Identificator", Order = 2)]
        public GebouwIdentificator Identificator { get; private set; }

        [JsonProperty("GebouwPolygoon", Order = 3)]
        public BuildingPolygon? GebouwPolygoon { get; private set; }

        [JsonProperty("GebouwStatus", Order = 4)]
        public GebouwStatus Status { get; private set; }

        [JsonProperty("IsVerwijderd", Order = 5)]
        public bool IsVerwijderd { get; private set; }

        [JsonProperty("Gebouweenheden", Order = 6)]
        public List<string> Gebouweenheden { get; private set; }

        public BuildingLdes(BuildingDetail building, IEnumerable<int> buildingUnitPersistentLocalIds, string osloNamespace)
        {
            Identificator = new GebouwIdentificator(osloNamespace, building.PersistentLocalId.ToString(), building.Version.ToBelgianDateTimeOffset());
            GebouwPolygoon = GetBuildingPolygon(building.Geometry, building.GeometryMethod);
            Status = building.Status.Map();
            IsVerwijderd = building.IsRemoved;
            Gebouweenheden = buildingUnitPersistentLocalIds.Select(x => x.ToString()).ToList();
        }

        private static BuildingPolygon? GetBuildingPolygon(byte[] polygon, BuildingGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(polygon) as Polygon;

            if (geometry == null) // some buildings have multi polygons (imported) which are incorrect.
            {
                return null;
            }

            var gml = GetGml(geometry);

            return new BuildingPolygon(new GmlJsonPolygon(gml), geometryMethod.Map());
        }

        private static string GetGml(Polygon polygon)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };

            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Polygon", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "http://www.opengis.net/def/crs/EPSG/0/31370");
                WriteRing((LinearRing)polygon.ExteriorRing, xmlwriter);
                WriteInteriorRings(polygon.InteriorRings, polygon.NumInteriorRings, xmlwriter);
                xmlwriter.WriteEndElement();
            }
            return builder.ToString();
        }

        private static void WriteRing(LinearRing ring, XmlWriter writer, bool isInterior = false)
        {
            writer.WriteStartElement("gml", isInterior ? "interior" : "exterior", "http://www.opengis.net/gml/3.2");
            writer.WriteStartElement("gml", "LinearRing", "http://www.opengis.net/gml/3.2");
            writer.WriteStartElement("gml", "posList", "http://www.opengis.net/gml/3.2");

            var posListBuilder = new StringBuilder();
            foreach (var coordinate in ring.Coordinates)
            {
                posListBuilder.Append(string.Format(
                    Global.GetNfi(),
                    "{0} {1} ",
                    coordinate.X.ToPolygonGeometryCoordinateValueFormat(),
                    coordinate.Y.ToPolygonGeometryCoordinateValueFormat()));
            }

            //remove last space
            posListBuilder.Length--;

            writer.WriteValue(posListBuilder.ToString());

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WriteInteriorRings(LineString[] rings, int numInteriorRings, XmlWriter writer)
        {
            if (numInteriorRings < 1)
            {
                return;
            }

            foreach (var ring in rings)
            {
                WriteRing((LinearRing)ring, writer, true);
            }
        }
    }

    public class BuildingPolygon
    {
        [JsonProperty("Geometrie", Order = 0)]
        public GmlJsonPolygon Geometry { get; set; }

        [JsonProperty("GeometrieMethode", Order = 1)]
        public GeometrieMethode GeometryMethod { get; set; }

        public BuildingPolygon(GmlJsonPolygon geometry, GeometrieMethode geometryMethod)
        {
            Geometry = geometry;
            GeometryMethod = geometryMethod;
        }
    }
}
