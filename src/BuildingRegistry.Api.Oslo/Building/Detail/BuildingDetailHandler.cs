namespace BuildingRegistry.Api.Oslo.Building.Detail
{
    using System;
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
    using BuildingRegistry.Api.Oslo.Converters;
    using BuildingRegistry.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;

    public class BuildingDetailHandler : IRequestHandler<BuildingDetailRequest, BuildingOsloResponseWithEtag>
    {
        public async Task<BuildingOsloResponseWithEtag> Handle(BuildingDetailRequest buildingDetailRequest, CancellationToken cancellationToken)
        {
            var building = await buildingDetailRequest.Context
                .BuildingDetails
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == buildingDetailRequest.PersistentLocalId, cancellationToken);

            if (building is { IsRemoved: true })
            {
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (building is not { IsComplete: true })
            {
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);
            }

            //TODO: improvement getting buildingunits and parcels in parallel.
            var buildingUnits = await buildingDetailRequest.Context
                .BuildingUnitDetails
                .Where(x => x.BuildingId == building.BuildingId)
                .Where(x => x.IsComplete && !x.IsRemoved)
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            var parcels = buildingDetailRequest.GrbBuildingParcel
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeys = await buildingDetailRequest.SyndicationContext
                .BuildingParcelLatestItems
                .Where(x => !x.IsRemoved &&
                            parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            return new BuildingOsloResponseWithEtag(
                new BuildingOsloResponse(
                    building.PersistentLocalId.Value,
                    buildingDetailRequest.ResponseOptions.Value.GebouwNaamruimte,
                    buildingDetailRequest.ResponseOptions.Value.ContextUrlDetail,
                    building.Version.ToBelgianDateTimeOffset(),
                    GetBuildingPolygon(building.Geometry, building.GeometryMethod.Value),
                    building.Status.Value.MapToGebouwStatus(),
                    buildingUnits.OrderBy(x => x.Value).Select(x => new GebouwDetailGebouweenheid(x.ToString(), string.Format(buildingDetailRequest.ResponseOptions.Value.GebouweenheidDetailUrl, x))).ToList(),
                    caPaKeys.Select(x => new GebouwDetailPerceel(x, string.Format(buildingDetailRequest.ResponseOptions.Value.PerceelUrl, x))).ToList()));
        }

        private static GeometrieMethode MapGeometryMethod(BuildingGeometryMethod geometryMethod)
        {
            switch (geometryMethod)
            {
                case BuildingGeometryMethod.Outlined:
                    return GeometrieMethode.Ingeschetst;

                case BuildingGeometryMethod.MeasuredByGrb:
                    return GeometrieMethode.IngemetenGRB;

                default:
                    throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
            }
        }

        private static BuildingPolygon GetBuildingPolygon(byte[] polygon, BuildingGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(polygon) as NetTopologySuite.Geometries.Polygon;

            if (geometry == null) //some buildings have multi polygons (imported) which are incorrect.
            {
                return null;
            }

            var gml = GetGml(geometry);

            return new BuildingPolygon(new GmlJsonPolygon(gml), MapGeometryMethod(geometryMethod));
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