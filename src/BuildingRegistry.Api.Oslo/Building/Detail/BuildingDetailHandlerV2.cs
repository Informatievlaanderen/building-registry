namespace BuildingRegistry.Api.Oslo.Building.Detail
{
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using BuildingRegistry.Building;
    using Consumer.Read.Parcel;
    using Converters;
    using Infrastructure.Grb;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using Projections.Legacy;
    using LinearRing = NetTopologySuite.Geometries.LinearRing;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    public class BuildingDetailHandlerV2 : IRequestHandler<BuildingDetailRequest, BuildingOsloResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IOptions<ResponseOptions> _responseOptions;
        private readonly IGrbBuildingParcel _grbBuildingParcel;

        public BuildingDetailHandlerV2(
            LegacyContext context,
            ConsumerParcelContext consumerParcelContext,
            IOptions<ResponseOptions> responseOptions,
            IGrbBuildingParcel grbBuildingParcel)
        {
            _context = context;
            _consumerParcelContext = consumerParcelContext;
            _responseOptions = responseOptions;
            _grbBuildingParcel = grbBuildingParcel;
        }

        public async Task<BuildingOsloResponseWithEtag> Handle(BuildingDetailRequest buildingDetailRequest, CancellationToken cancellationToken)
        {
            var building = await _context
                .BuildingDetailsV2
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == buildingDetailRequest.PersistentLocalId, cancellationToken);

            if (building is { IsRemoved: true })
            {
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (building is null)
            {
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);
            }

            var buildingUnitsTask = _context
                .BuildingUnitDetailsV2
                .Where(x => x.BuildingPersistentLocalId == building.PersistentLocalId)
                .Where(x => !x.IsRemoved)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToListAsync(cancellationToken);

            var parcels = _grbBuildingParcel
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeysTask = _consumerParcelContext
                .ParcelConsumerItems
                .Where(x => !x.IsRemoved && parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(buildingUnitsTask, caPaKeysTask);

            var buildingUnits = buildingUnitsTask.Result;
            var caPaKeys = caPaKeysTask.Result;

            return new BuildingOsloResponseWithEtag(
                new BuildingOsloResponse(
                    building.PersistentLocalId,
                    _responseOptions.Value.GebouwNaamruimte,
                    _responseOptions.Value.ContextUrlDetail,
                    building.Version.ToBelgianDateTimeOffset(),
                    GetBuildingPolygon(building.Geometry, building.GeometryMethod),
                    building.Status.Map(),
                    buildingUnits.OrderBy(x => x).Select(x => new GebouwDetailGebouweenheid(x.ToString(), string.Format(_responseOptions.Value.GebouweenheidDetailUrl, x))).ToList(),
                    caPaKeys.Select(x => new GebouwDetailPerceel(x, string.Format(_responseOptions.Value.PerceelUrl, x))).ToList()),
                building.LastEventHash);
        }

        private static BuildingPolygon GetBuildingPolygon(byte[] polygon, BuildingGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(polygon) as Polygon;

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

            var polygon = geometry as Polygon;

            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Polygon", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
                WriteRing(polygon.ExteriorRing as LinearRing, xmlwriter);
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
                WriteRing(ring as LinearRing, writer, true);
            }
        }
    }
}
