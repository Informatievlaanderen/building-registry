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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using BuildingRegistry.Api.Oslo.Converters;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using Projections.Legacy;
    using Projections.Syndication;
    using BuildingUnitFunction = Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = Legacy.BuildingUnitPositionGeometryMethod;

    public class GetHandler : IRequestHandler<GetRequest, BuildingUnitOsloResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetHandler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitOsloResponseWithEtag> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var buildingUnit = await _context
                .BuildingUnitDetails
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (buildingUnit is { IsRemoved: true })
            {
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (buildingUnit is not { IsComplete: true, IsBuildingComplete: true })
            {
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);
            }

            var addressIds = buildingUnit.Addresses.Select(x => x.AddressId).ToList();
            var addressPersistentLocalIds = await _syndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            return new BuildingUnitOsloResponseWithEtag(
                new BuildingUnitOsloResponse(
                    buildingUnit.PersistentLocalId.Value,
                    _responseOptions.Value.GebouweenheidNaamruimte,
                    _responseOptions.Value.ContextUrlUnitDetail,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    GetBuildingUnitPoint(buildingUnit.Position, buildingUnit.PositionMethod.Value),
                    buildingUnit.Status.Value.Map(),
                    MapBuildingUnitFunction(buildingUnit.Function),
                    new GebouweenheidDetailGebouw(buildingUnit.BuildingPersistentLocalId.Value.ToString(), string.Format(_responseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId.Value)),
                    addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(_responseOptions.Value.AdresUrl, id))).ToList(),
                    false));
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

        private static Detail.BuildingUnitPosition GetBuildingUnitPoint(byte[] point, BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            var gml = GetGml(geometry);
            return new Detail.BuildingUnitPosition(new GmlJsonPoint(gml), MapBuildingUnitGeometryMethod(geometryMethod));
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
            writer.WriteValue(string.Format(NetTopologySuite.Utilities.Global.GetNfi(), "{0} {1}",
                coordinate.X.ToPointGeometryCoordinateValueFormat(), coordinate.Y.ToPointGeometryCoordinateValueFormat()));
            writer.WriteEndElement();
        }
    }
}
