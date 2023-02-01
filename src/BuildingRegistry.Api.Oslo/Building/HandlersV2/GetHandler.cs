namespace BuildingRegistry.Api.Oslo.Building.HandlersV2
{
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using BuildingRegistry;
    using BuildingRegistry.Building;
    using Converters;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using Requests;
    using Responses;

    public class GetHandler : IRequestHandler<GetRequest, BuildingOsloResponseWithEtag>
    {
        public async Task<BuildingOsloResponseWithEtag> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var building = await request.Context
                .BuildingDetailsV2
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (building is { IsRemoved: true })
            {
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (building is null)
            {
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);
            }

            var buildingUnitsTask = request.Context
                .BuildingUnitDetailsV2
                .Where(x => x.BuildingPersistentLocalId == building.PersistentLocalId)
                .Where(x => !x.IsRemoved)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToListAsync(cancellationToken);

            var parcels = request.GrbBuildingParcel
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeysTask = request.SyndicationContext
                .BuildingParcelLatestItems
                .Where(x => !x.IsRemoved && parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(buildingUnitsTask, caPaKeysTask);

            var buildingUnits = buildingUnitsTask.Result;
            var caPaKeys = caPaKeysTask.Result;

            return new BuildingOsloResponseWithEtag(
                new BuildingOsloResponse(
                    building.PersistentLocalId,
                    request.ResponseOptions.Value.GebouwNaamruimte,
                    request.ResponseOptions.Value.ContextUrlDetail,
                    building.Version.ToBelgianDateTimeOffset(),
                    GetBuildingPolygon(building.Geometry, building.GeometryMethod),
                    building.Status.Map(),
                    buildingUnits.OrderBy(x => x).Select(x => new GebouwDetailGebouweenheid(x.ToString(), string.Format(request.ResponseOptions.Value.GebouweenheidDetailUrl, x))).ToList(),
                    caPaKeys.Select(x => new GebouwDetailPerceel(x, string.Format(request.ResponseOptions.Value.PerceelUrl, x))).ToList()),
                building.LastEventHash);
        }

        private static BuildingPolygon GetBuildingPolygon(byte[] polygon, BuildingGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(polygon) as NetTopologySuite.Geometries.Polygon;

            if (geometry == null) //some buildings have multi polygons (imported) which are incorrect.
            {
                return null;
            }

            var gml = GetGml(geometry);

            return new BuildingPolygon(new GmlJsonPolygon(gml), geometryMethod.Map());
        }

        internal static string GetGml(Geometry geometry)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };

            var polygon = geometry as NetTopologySuite.Geometries.Polygon;

            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Polygon", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
                WriteRing(polygon.ExteriorRing as NetTopologySuite.Geometries.LinearRing, xmlwriter);
                WriteInteriorRings(polygon.InteriorRings, polygon.NumInteriorRings, xmlwriter);
                xmlwriter.WriteEndElement();
            }
            return builder.ToString();
        }

        private static void WriteRing(NetTopologySuite.Geometries.LinearRing ring, XmlWriter writer, bool isInterior = false)
        {
            writer.WriteStartElement("gml", isInterior ? "interior" : "exterior", "http://www.opengis.net/gml/3.2");
            writer.WriteStartElement("gml", "LinearRing", "http://www.opengis.net/gml/3.2");
            writer.WriteStartElement("gml", "posList", "http://www.opengis.net/gml/3.2");

            var posListBuilder = new StringBuilder();
            foreach (var coordinate in ring.Coordinates)
            {
                posListBuilder.Append(string.Format(
                    NetTopologySuite.Utilities.Global.GetNfi(),
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
                WriteRing(ring as NetTopologySuite.Geometries.LinearRing, writer, true);
            }
        }
    }
}
