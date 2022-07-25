namespace BuildingRegistry.Api.Oslo.Handlers.BuildingUnit
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Abstractions.BuildingUnit;
    using Abstractions.Converters;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using BuildingRegistry.Api.Oslo.Abstractions.BuildingUnit.Responses;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using BuildingUnitFunction = Legacy.BuildingUnitFunction;
    using BuildingUnitPosition = Abstractions.BuildingUnit.Responses.BuildingUnitPosition;
    using BuildingUnitPositionGeometryMethod = Legacy.BuildingUnitPositionGeometryMethod;

    public class GetHandler : IRequestHandler<GetRequest, BuildingUnitOsloResponse>
    {
        public async Task<BuildingUnitOsloResponse> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var buildingUnit = await request.Context
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
            var addressPersistentLocalIds = await request.SyndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            return new BuildingUnitOsloResponse(
                buildingUnit.PersistentLocalId.Value,
                request.ResponseOptions.Value.GebouweenheidNaamruimte,
                request.ResponseOptions.Value.ContextUrlUnitDetail,
                buildingUnit.Version.ToBelgianDateTimeOffset(),
                GetBuildingUnitPoint(buildingUnit.Position, buildingUnit.PositionMethod.Value),
                buildingUnit.Status.Value.Map(),
                MapBuildingUnitFunction(buildingUnit.Function),
                new GebouweenheidDetailGebouw(
                    buildingUnit.PersistentLocalId.Value.ToString(),
                    string.Format(request.ResponseOptions.Value.GebouweenheidDetailUrl, buildingUnit.PersistentLocalId.Value)),
                addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(request.ResponseOptions.Value.AdresUrl, id))).ToList());
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
            writer.WriteValue(string.Format(NetTopologySuite.Utilities.Global.GetNfi(), "{0} {1}",
                coordinate.X.ToPointGeometryCoordinateValueFormat(), coordinate.Y.ToPointGeometryCoordinateValueFormat()));
            writer.WriteEndElement();
        }
    }
}
