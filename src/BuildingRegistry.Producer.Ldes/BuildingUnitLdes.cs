namespace BuildingRegistry.Producer.Ldes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Building;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class BuildingUnitLdes
    {
        // Removed this part because the LDES server can't handle reverse properties
        // ""gebouw"": {
        //   ""@reverse"": ""https://data.vlaanderen.be/ns/gebouw#bestaatUit"",
        //   ""@type"": ""@id"",
        //   ""@context"": {
        //     ""@base"": ""https://data.vlaanderen.be/id/gebouw/""
        //   }
        // },
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
  ""gebouweenheidStatus"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouweenheid.status"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/doc/concept/gebouweenheidstatus/""
    }
  },
  ""functie"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#functie"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/doc/concept/gebouweenheidfunctie/""
    }
  },
  ""afwijkingvastgesteld"": {
    ""@id"": ""https://basisregisters.vlaanderen.be/implementatiemodel/gebouwenregister#afwijkingvastgesteld"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#boolean""
  },
  ""adressen"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouweenheid.adres"",
    ""@container"": ""@set"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/id/adres/""
    }
  },
  ""gebouweenheidPositie"": {
    ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouweenheid.geometrie"",
    ""@type"": ""@id"",
    ""@context"": {
      ""positieGeometrieMethode"": {
        ""@id"": ""https://data.vlaanderen.be/id/conceptscheme/geometriemethode"",
        ""@type"": ""@id"",
        ""@context"": {
          ""@base"": ""https://data.vlaanderen.be/doc/concept/geometriemethode/""
        }
      },
      ""geometrie"": {
        ""@id"": ""https://www.w3.org/ns/locn#geometry"",
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
  }
}");

        [JsonProperty("@context", Order = 0)]
        public JObject LdContext => Context;

        [JsonProperty("@type", Order = 1)]
        public string Type => "Gebouweenheid";

        [JsonProperty("Identificator", Order = 2)]
        public GebouweenheidIdentificator Identificator { get; private set; }

        [JsonProperty("GebouweenheidPositie", Order = 3)]
        public BuildingUnitPosition GebouweenheidPositie { get; private set; }

        [JsonProperty("GebouweenheidStatus", Order = 4)]
        public GebouweenheidStatus Status { get; private set; }

        [JsonProperty("Functie", Order = 5)]
        public GebouweenheidFunctie Functie { get; private set; }

        [JsonProperty("IsVerwijderd", Order = 6)]
        public bool IsVerwijderd { get; private set; }

        // [JsonProperty("Gebouw", Order = 7)]
        // public string GebouwId { get; private set; }

        [JsonProperty("Adressen", Order = 8)]
        public List<string> Adressen { get; private set; }

        [JsonProperty("AfwijkingVastgesteld", Order = 9)]
        public bool AfwijkingVastgesteld { get; private set; }

        public BuildingUnitLdes(BuildingUnitDetail buildingUnit, string osloNamespace)
        {
            Identificator = new GebouweenheidIdentificator(osloNamespace, buildingUnit.BuildingUnitPersistentLocalId.ToString(), buildingUnit.Version.ToBelgianDateTimeOffset());
            GebouweenheidPositie = GetBuildingUnitPoint(buildingUnit.Position, buildingUnit.PositionMethod);
            Status = buildingUnit.Status.Map();
            Functie = buildingUnit.Function.Map();
            IsVerwijderd = buildingUnit.IsRemoved;
            //GebouwId = buildingUnit.BuildingPersistentLocalId.ToString();
            Adressen = buildingUnit.Addresses.Select(x => x.AddressPersistentLocalId.ToString()).ToList();
            AfwijkingVastgesteld = buildingUnit.HasDeviation;
        }

        private static BuildingUnitPosition GetBuildingUnitPoint(byte[] point, BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            var gml = GetGml(geometry);
            return new BuildingUnitPosition(new GmlJsonPoint(gml), geometryMethod.Map());
        }

        private static string GetGml(Geometry geometry)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Point", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "http://www.opengis.net/def/crs/EPSG/0/31370");
                Write(geometry.Coordinate, xmlwriter);
                xmlwriter.WriteEndElement();
            }

            return builder.ToString();
        }

        private static void Write(Coordinate coordinate, XmlWriter writer)
        {
            writer.WriteStartElement("gml", "pos", "http://www.opengis.net/gml/3.2");
            writer.WriteValue(string.Format(Global.GetNfi(), "{0} {1}",
                coordinate.X.ToPointGeometryCoordinateValueFormat(), coordinate.Y.ToPointGeometryCoordinateValueFormat()));
            writer.WriteEndElement();
        }
    }

    public class BuildingUnitPosition
    {
        [JsonProperty("Geometrie", Order = 0)]
        public GmlJsonPoint Geometry { get; set; }

        [JsonProperty("PositieGeometrieMethode", Order = 1)]
        public PositieGeometrieMethode GeometryMethod { get; set; }

        public BuildingUnitPosition(GmlJsonPoint geometry, PositieGeometrieMethode geometryMethod)
        {
            Geometry = geometry;
            GeometryMethod = geometryMethod;
        }
    }
}
