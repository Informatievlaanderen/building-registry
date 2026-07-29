namespace BuildingRegistry.Api.Oslo.Building.V3.Detail
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
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw;
    using BuildingRegistry.Building;
    using Consumer.Read.Parcel;
    using Converters;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;
    using Polygon = NetTopologySuite.Geometries.Polygon;
    using WKBReaderFactory = BuildingRegistry.WKBReaderFactory;

    public class BuildingDetailHandler : IRequestHandler<BuildingDetailRequest, BuildingOsloResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;
        private readonly IParcelMatching _parcelMatching;

        public BuildingDetailHandler(
            LegacyContext context,
            ConsumerParcelContext consumerParcelContext,
            IOptions<ResponseOptionsV3> responseOptions,
            IParcelMatching parcelMatching)
        {
            _context = context;
            _consumerParcelContext = consumerParcelContext;
            _responseOptions = responseOptions;
            _parcelMatching = parcelMatching;
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
                .BuildingUnitDetailsV2WithCount
                .Where(x => x.BuildingPersistentLocalId == building.PersistentLocalId)
                .Where(x => !x.IsRemoved)
                .Select(x => new {x.BuildingUnitPersistentLocalId, x.Status})
                .ToListAsync(cancellationToken);

            var parcels = _parcelMatching
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeysTask = _consumerParcelContext
                .ParcelConsumerItemsWithCount
                .Where(x => !x.IsRemoved && parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(buildingUnitsTask, caPaKeysTask);

            var buildingUnits = buildingUnitsTask.Result;
            var caPaKeys = caPaKeysTask.Result;

            return new BuildingOsloResponseWithEtag(
                new BuildingOsloV3Response(
                    building.PersistentLocalId,
                    _responseOptions.Value.ContextUrlDetail,
                    building.Version.ToBelgianDateTimeOffset(),
                    GetBuildingPolygon(building.Geometry, building.GeometryMethod),
                    building.Status.MapOslo(),
                    buildingUnits
                        .OrderBy(x => x.BuildingUnitPersistentLocalId)
                        .Select(x =>
                            new GebouwBestaatUitGebouweenheid(
                                OsloNamespaces.Gebouweenheid.ToPuri(x.BuildingUnitPersistentLocalId.ToString()),
                                x.Status.MapOslo(),
                                new Uri(string.Format(_responseOptions.Value.GebouweenheidDetailUrl, x.BuildingUnitPersistentLocalId)))).ToList(),
                    caPaKeys.Select(x => new GebouwLigtOpPerceel(OsloNamespaces.Perceel.ToPuri(x), new Uri(string.Format(_responseOptions.Value.PerceelUrl, x)))).ToList()),
                building.LastEventHash);
        }

        private static BuildingPolygonV3? GetBuildingPolygon(byte[] polygon, BuildingGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(polygon) as Polygon;

            if (geometry == null) //some buildings have multi polygons (imported) which are incorrect.
            {
                return null;
            }

            var gmls = new List<string>();

            if(polygon.TryReadSrid(out var srid))
            {
                if (srid == SystemReferenceId.SridLambert2008)
                {
                    var transformFromLambert08To72 = geometry.TransformFromLambert08To72();
                    gmls.Add(GeometryExtensions.ConvertToGml(transformFromLambert08To72, false));
                }
            }
            gmls.Add(GeometryExtensions.ConvertToGml(geometry, false));


            return new BuildingPolygonV3(gmls, geometryMethod.MapOslo());
        }
    }
}
