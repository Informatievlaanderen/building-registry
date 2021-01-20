namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;

    public class GrbWfsClient : IGrbWfsClient
    {
        internal const string GmlNameSpace = "http://www.opengis.net/gml";
        internal const string WfsNameSpace = "http://www.opengis.net/wfs";
        internal const string GrbNameSpace = "informatievlaanderen.be/grb";
        internal const string GrbWfs = "https://geoservices.informatievlaanderen.be/overdrachtdiensten/GRB/wfs"; // TODO place in config file

        private readonly XmlNamespaceManager _namespaceManager;
        private readonly GMLReader _gmlReader;

        public GrbWfsClient()
        {
            _namespaceManager = new XmlNamespaceManager(new NameTable());
            _namespaceManager.AddNamespace("wfs", WfsNameSpace);
            _namespaceManager.AddNamespace("gml", GmlNameSpace);
            _namespaceManager.AddNamespace("grb", GrbNameSpace);

            _gmlReader = new GMLReader();
        }

        public IEnumerable<Tuple<Geometry, IReadOnlyDictionary<string, string>>> GetFeaturesInBoundingBox(GrbFeatureType type, Envelope boundingBox)
        {
            var featureName = GrbFeatureName.For(type);
            var wfsRequest = CreateWfsRequest(featureName, boundingBox);

            try
            {
                var response = wfsRequest
                        .GetResponse()
                        .GetResponseStream();

                var wfsDoc = XDocument.Load(response);
                if (wfsDoc.Root?.Name?.LocalName == null)
                    throw new WfsException("Invalid response");

                if (wfsDoc.Root.Name.LocalName.Equals("ServiceExceptionReport", StringComparison.OrdinalIgnoreCase))
                    throw new WfsException(wfsDoc.ToString());

                return wfsDoc
                    .XPathSelectElements("/wfs:FeatureCollection/gml:featureMember", _namespaceManager)
                    .Select(element => MapFeature(featureName, element));
            }
            catch (Exception exception)
                when (exception is WfsException)
            {
                throw new WfsException(exception);
            }
        }

        private static WebRequest CreateWfsRequest(GrbFeatureName featureName, Envelope boundingBox)
        {
            static string Format(double val) => val.ToString(CultureInfo.InvariantCulture.NumberFormat);

            var payload = string.Format(
                "<wfs:GetFeature xmlns:wfs=\"http://www.opengis.net/wfs\" service=\"WFS\" version=\"1.0.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.0.0/WFS-transaction.xsd\">" +
                    "<wfs:Query typeName=\"grb:{0}\" xmlns:grb=\"informatievlaanderen.be/grb\">" +
                        "<ogc:Filter xmlns:ogc=\"http://www.opengis.net/ogc\">" +
                            "<ogc:BBOX><ogc:PropertyName>SHAPE</ogc:PropertyName>" +
                                "<gml:Box xmlns:gml=\"http://www.opengis.net/gml\" srsName=\"EPSG:31370\">" +
                                    "<gml:coordinates decimal=\".\" cs=\",\" ts=\" \">{1},{2} {3},{4}</gml:coordinates>" +
                                "</gml:Box>" +
                            "</ogc:BBOX>" +
                        "</ogc:Filter>" +
                    "</wfs:Query>" +
                "</wfs:GetFeature>",
                featureName,
                Format(boundingBox.MinX),
                Format(boundingBox.MinY),
                Format(boundingBox.MaxX),
                Format(boundingBox.MaxY));

            var request = WebRequest.Create(GrbWfs); // TODO: put this in config?
            request.Method = HttpMethod.Post.Method;
            ((HttpWebRequest)request).ContentType = "application/xml";
            using (var writer = new StreamWriter(request.GetRequestStream()))
                writer.Write(payload);

            return request;
        }

        private Tuple<Geometry, IReadOnlyDictionary<string, string>> MapFeature(GrbFeatureName name, XElement data)
        {
            var featureElement = data.Element(XName.Get(name.ToString(), GrbNameSpace));

            var gml = featureElement
                .Element(XName.Get("SHAPE", GrbNameSpace))
                .Elements()
                .First();

            if (gml.Name.LocalName == "MultiPolygon")
                gml = gml
                    .Element(XName.Get("polygonMember", GmlNameSpace))
                    .Element(XName.Get("Polygon", GmlNameSpace));

            gml
                .DescendantsAndSelf()
                .ToList()
                .ForEach(d => d.RemoveAttributes());

            // convert GML2 to GML3
            //var gmlString = gml.ToString().Replace("outerBoundaryIs", "exterior").Replace("innerBoundaryIs", "interior").Replace("coordinates", "posList").Replace(",", " ");

            var geometry = _gmlReader.Read(gml.ToString());

            var attributes = featureElement
                .Elements()
                .Where(el => el.Name.Namespace == GrbNameSpace && el.Name.LocalName != "SHAPE")
                .ToDictionary(el => el.Name.LocalName, el => el.Value) as IReadOnlyDictionary<string,string>;

            return Tuple.Create(geometry, attributes);
        }
    }
}
