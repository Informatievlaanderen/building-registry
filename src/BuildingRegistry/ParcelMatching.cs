namespace BuildingRegistry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Building;
    using NetTopologySuite.Geometries;

    public class ParcelMatching : IParcelMatching
    {
        private readonly IParcels _parcels;
        private readonly Lambert2008ConversionCompletedToggle _conversionCompleted;
        private readonly Lambert2008MatchingReadiness _readiness;

        public ParcelMatching(
            IParcels parcels,
            Lambert2008ConversionCompletedToggle conversionCompleted,
            Lambert2008MatchingReadiness readiness)
        {
            _parcels = parcels;
            _conversionCompleted = conversionCompleted;
            _readiness = readiness;
        }

        /// <summary>
        /// Finds the parcels a building overlaps.
        ///
        /// The building geometry is brought to the reference system matching is done in, and the parcels
        /// come back in that same system, so the SQL bounding-box filter and the in-memory NTS overlay below
        /// compare like with like. Neither would say otherwise if they did not: SQL Server returns NULL on an
        /// SRID mismatch and NTS ignores SRID altogether. See ADR 0006.
        ///
        /// The incoming geometry is transformed for comparison only; what a command persists is unaffected.
        /// </summary>
        public async Task<IEnumerable<ParcelData>> GetUnderlyingParcels(Geometry geometry)
        {
            if (_conversionCompleted.FeatureEnabled)
            {
                _readiness.EnsureVerified(() => _parcels.HasIncompleteLambert2008Geometry().GetAwaiter().GetResult());
            }

            var matchingSrid = _conversionCompleted.MatchingSrid;
            var matchingGeometry = ToMatchingCrs(geometry, matchingSrid);

            var underlyingParcels = (await _parcels.GetUnderlyingParcelsUnderBoundingBox(matchingGeometry, matchingSrid))
                .Where(parcel => matchingGeometry.Intersects(parcel.Geometry) && parcel.Status == "Realized")
                .Select(parcel =>
                    new
                    {
                        Overlap = CalculateOverlap(matchingGeometry, parcel.Geometry),
                        ParcelData = parcel
                    })
                .ToList();

            var overlappingParcels = underlyingParcels
                .Where(parcel => parcel.Overlap >= 0.8 / underlyingParcels.Count)
                .Select(x => x.ParcelData);

            return overlappingParcels;
        }

        private static Geometry ToMatchingCrs(Geometry geometry, int matchingSrid)
            => matchingSrid == SystemReferenceId.SridLambert2008
                ? geometry.IsLambert08() ? geometry : geometry.EnsureLambert08(2)
                : geometry.IsLambert72() ? geometry : geometry.EnsureLambert72().RoundCoordinates(2);

        private static double CalculateOverlap(Geometry building, Geometry parcel)
        {
            try
            {
                return building.Intersection(parcel).Area / building.Area;
            }
            catch (TopologyException topologyException)
            {
                // Consider parcels that Intersect, but fail with "found non-noded intersection" on calculating, to have an overlap value of 0
                if (topologyException.Message.Contains("found non-noded intersection", StringComparison.InvariantCultureIgnoreCase))
                    return 0;

                // any other TopologyException should be treated normally
                throw;
            }
        }
    }
}
