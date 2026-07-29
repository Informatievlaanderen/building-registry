namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid;
    using BuildingRegistry.Building;
    using Converters;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;
    using WKBReaderFactory = BuildingRegistry.WKBReaderFactory;

    public class BuildingUnitDetailHandler : IRequestHandler<GetRequest, BuildingUnitOsloResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public BuildingUnitDetailHandler(
            LegacyContext context,
            IOptions<ResponseOptionsV3> responseOptions)
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
                new BuildingUnitOsloV3Response(
                    buildingUnit.BuildingUnitPersistentLocalId,
                    _responseOptions.Value.ContextUrlUnitDetail,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    GetBuildingUnitPoint(buildingUnit.Position, buildingUnit.PositionMethod),
                    buildingUnit.Status.MapOslo(),
                    MapBuildingUnitFunction(buildingUnit.Function),
                    new GebouweenheidIsDeelVan(
                        OsloNamespaces.Gebouw.ToPuri(buildingUnit.BuildingPersistentLocalId.ToString()),
                        new Uri(string.Format(_responseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId))),
                    addressPersistentLocalIds
                        .Select(id => new GebouweenheidToegekendAdres(OsloNamespaces.Adres.ToPuri(id.ToString()), new Uri(string.Format(_responseOptions.Value.AdresUrl, id)))).ToList(),
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

        private static GebouweenheidFunctieValue MapBuildingUnitFunction(BuildingUnitFunction function)
        {
            if (BuildingUnitFunction.Common == function)
            {
                return GebouweenheidFunctieValue.GemeenschappelijkDeel;
            }

            if (BuildingUnitFunction.Unknown == function)
            {
                return GebouweenheidFunctieValue.NietGekend;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        private static BuildingUnitPositionV3 GetBuildingUnitPoint(byte[] point, BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            var gmls = new List<string>();

            if(point.TryReadSrid(out var srid))
            {
                if (srid == SystemReferenceId.SridLambert2008)
                {
                    var transformFromLambert08To72 = geometry.TransformFromLambert08To72();
                    gmls.Add(GeometryExtensions.ConvertToGml(transformFromLambert08To72, false));
                }
            }
            gmls.Add(GeometryExtensions.ConvertToGml(geometry, false));
            return new BuildingUnitPositionV3(gmls, MapBuildingUnitGeometryMethod(geometryMethod));
        }
    }
}
