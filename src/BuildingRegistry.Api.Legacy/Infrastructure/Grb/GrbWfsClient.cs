namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;

    public class GrbWfsClient : IGrbWfsClient
    {
        private readonly XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());
        private readonly GMLReader _gmlReader = new GMLReader();

        private string AdpName = "ADP";
        private string WtzName = "GRB_-_Wtz_-_watergang";

        internal const string GmlNameSpace = "http://www.opengis.net/gml";
        internal const string WfsNameSpace = "http://www.opengis.net/wfs";
        internal const string GrbNameSpace = "informatievlaanderen.be/grb";
        internal const string GrbWfs = "https://geoservices.informatievlaanderen.be/overdrachtdiensten/GRB/wfs"; // TODO place in config file

        public GrbWfsClient()
        {
            _namespaceManager.AddNamespace("wfs", WfsNameSpace);
            _namespaceManager.AddNamespace("gml", GmlNameSpace);
            _namespaceManager.AddNamespace("grb", GrbNameSpace);
        }

        public IEnumerable<Tuple<Geometry, IReadOnlyDictionary<string, string>>> GetFeaturesInBoundingBox(GrbFeatureType featureType, Envelope boundingBox)
        {
            var result = new List<Tuple<Geometry, IReadOnlyDictionary<string, string>>>();

            string featureName;
            switch (featureType)
            {
                case GrbFeatureType.Parcel:
                    featureName = AdpName;
                    break;

                case GrbFeatureType.Waterway:
                    featureName = WtzName;
                    break;

                default:
                    throw new ArgumentException(
                        $"Name of GRB feature type [{Enum.GetName(typeof(GrbFeatureType), featureType)}] cannot be resolved.");
            }

            var wfsRequest = WebRequest.Create(GrbWfs); // TODO: put this in config?
            wfsRequest.Method = HttpMethod.Post.Method;
            ((HttpWebRequest)wfsRequest).ContentType = "application/xml";

            using (var writer = new StreamWriter(wfsRequest.GetRequestStream()))
            {
                var payload = string.Format(
                    "<wfs:GetFeature xmlns:wfs=\"http://www.opengis.net/wfs\" service=\"WFS\" version=\"1.0.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.0.0/WFS-transaction.xsd\"><wfs:Query typeName=\"grb:{0}\" xmlns:grb=\"informatievlaanderen.be/grb\"><ogc:Filter xmlns:ogc=\"http://www.opengis.net/ogc\"><ogc:BBOX><ogc:PropertyName>SHAPE</ogc:PropertyName><gml:Box xmlns:gml=\"http://www.opengis.net/gml\" srsName=\"EPSG:31370\"><gml:coordinates decimal=\".\" cs=\",\" ts=\" \">{1},{2} {3},{4}</gml:coordinates></gml:Box></ogc:BBOX></ogc:Filter></wfs:Query></wfs:GetFeature>",
                    featureName,
                    boundingBox.MinX.ToString(CultureInfo.InvariantCulture.NumberFormat),
                    boundingBox.MinY.ToString(CultureInfo.InvariantCulture.NumberFormat),
                    boundingBox.MaxX.ToString(CultureInfo.InvariantCulture.NumberFormat),
                    boundingBox.MaxY.ToString(CultureInfo.InvariantCulture.NumberFormat));

                writer.Write(payload);
            }

            var wfsResponse = wfsRequest.GetResponse();
            var wfsDoc = XDocument.Load(wfsResponse.GetResponseStream());

            if (wfsDoc.Root.Name.LocalName.Equals("ServiceExceptionReport", StringComparison.OrdinalIgnoreCase))
                throw new WfsException(wfsDoc.ToString());

            foreach (var adpFeature in wfsDoc.XPathSelectElements("/wfs:FeatureCollection/gml:featureMember", _namespaceManager))
            {
                var gml = adpFeature.Element(XName.Get(featureName, GrbNameSpace)).Element(XName.Get("SHAPE", GrbNameSpace)).Elements().First();

                var attributes = adpFeature.Element(XName.Get(featureName, GrbNameSpace)).Elements()
                    .Where(el => el.Name.Namespace == GrbNameSpace && el.Name.LocalName != "SHAPE")
                    .ToDictionary(el => el.Name.LocalName, el => el.Value);

                if (gml.Name.LocalName == "MultiPolygon")
                    gml = gml.Element(XName.Get("polygonMember", GmlNameSpace)).Element(XName.Get("Polygon", GmlNameSpace));

                gml.DescendantsAndSelf().ToList().ForEach(d => d.RemoveAttributes());

                // convert GML2 to GML3
                //var gmlString = gml.ToString().Replace("outerBoundaryIs", "exterior").Replace("innerBoundaryIs", "interior").Replace("coordinates", "posList").Replace(",", " ");

                var geometry = _gmlReader.Read(gml.ToString());

                result.Add(Tuple.Create(geometry, attributes as IReadOnlyDictionary<string, string>));
            }

            return result;
        }
    }

    public class WfsException : Exception
    {
        public WfsException()
        {
        }

        public WfsException(string message) : base(message)
        {
        }

        public WfsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WfsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
