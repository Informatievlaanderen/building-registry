namespace BuildingRegistry.Api.Oslo.BuildingUnit.Detail
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using BuildingRegistry.Building;
    using Converters;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using Projections.Legacy;

    public class BuildingUnitDetailHandlerV2 : IRequestHandler<GetRequest, BuildingUnitOsloResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public BuildingUnitDetailHandlerV2(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitOsloResponseWithEtag> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var buildingUnit = await _context
                .BuildingUnitDetailsV2WithCount
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.BuildingUnitPersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (buildingUnit is null)
            {
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);
            }

            if (buildingUnit is { IsRemoved: true })
            {
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);
            }

            var addressPersistentLocalIds = buildingUnit.Addresses
                .Select(x => x.AddressPersistentLocalId).ToList();

            return new BuildingUnitOsloResponseWithEtag(
                new BuildingUnitOsloResponse(
                    buildingUnit.BuildingUnitPersistentLocalId,
                    _responseOptions.Value.GebouweenheidNaamruimte,
                    _responseOptions.Value.ContextUrlUnitDetail,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    GetBuildingUnitPoint(buildingUnit.Position, buildingUnit.PositionMethod),
                    buildingUnit.Status.Map(),
                    MapBuildingUnitFunction(buildingUnit.Function),
                    new GebouweenheidDetailGebouw(
                        buildingUnit.BuildingPersistentLocalId.ToString(),
                        string.Format(_responseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId)),
                    addressPersistentLocalIds
                        .Select(id => new GebouweenheidDetailAdres(id.ToString(), string.Format(_responseOptions.Value.AdresUrl, id))).ToList(),
                    buildingUnit.HasDeviation),
                buildingUnit.LastEventHash);
        }

        private static PositieGeometrieMethode MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod geometryMethod)
        {
            if (BuildingUnitPositionGeometryMethod.AppointedByAdministrator == geometryMethod)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            if (BuildingUnitPositionGeometryMethod.DerivedFromObject == geometryMethod)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }

        private static GebouweenheidFunctie? MapBuildingUnitFunction(BuildingUnitFunction? function)
        {
            if (function == null)
            {
                return null;
            }

            if (BuildingUnitFunction.Common == function)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            if (BuildingUnitFunction.Unknown == function)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        private static BuildingUnitPosition GetBuildingUnitPoint(byte[] point, BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            var gml = GetGml(geometry);
            return new BuildingUnitPosition(new GmlJsonPoint(gml), MapBuildingUnitGeometryMethod(geometryMethod));
        }

        private static string GetGml(Geometry geometry)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Point", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
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
}
