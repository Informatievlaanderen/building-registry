namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Overlay;
    using NetTopologySuite.Operation.OverlayNG;
    using ParcelWithCount;

    public class ParcelMatching : IParcelMatching
    {
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly Lambert2008ConversionCompletedToggle _conversionCompleted;
        private readonly Lambert2008MatchingReadiness _readiness;

        public ParcelMatching(
            ConsumerParcelContext consumerParcelContext,
            Lambert2008ConversionCompletedToggle conversionCompleted,
            Lambert2008MatchingReadiness readiness)
        {
            _consumerParcelContext = consumerParcelContext;
            _conversionCompleted = conversionCompleted;
            _readiness = readiness;
        }

        /// <summary>
        /// Finds the CaPaKeys of the parcels a building overlaps.
        ///
        /// The building bytes are read in whatever reference system they carry and then brought to the one
        /// matching is done in, so the SQL bounding-box filter and the in-memory NTS overlay below compare
        /// like with like. Neither reports a mismatch: SQL Server returns NULL, and NTS ignores SRID and
        /// simply finds an empty intersection ~500 km away. See ADR 0006.
        /// </summary>
        public IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes)
        {
            var matchingSrid = _conversionCompleted.MatchingSrid;
            var useLambert2008 = matchingSrid == SystemReferenceId.SridLambert2008;

            if (useLambert2008)
            {
                _readiness.EnsureVerified(
                    () => _consumerParcelContext.HasIncompleteLambert2008Geometry().GetAwaiter().GetResult());
            }

            var buildingGeometry = ToMatchingCrs(
                WKBReaderFactory.CreateForEwkb(buildingGeometryBytes).Read(buildingGeometryBytes),
                matchingSrid);

            var boundingBox = buildingGeometry.Factory.ToGeometry(buildingGeometry.EnvelopeInternal);

            var candidates = useLambert2008
                ? _consumerParcelContext.ParcelConsumerItemsWithCount
                    .Where(parcel => boundingBox.Intersects(parcel.GeometryLambert2008))
                    .ToList()
                : _consumerParcelContext.ParcelConsumerItemsWithCount
                    .Where(parcel => boundingBox.Intersects(parcel.Geometry))
                    .ToList();

            var underlyingParcels = candidates
                .Select(parcel => new { parcel.CaPaKey, parcel.Status, Geometry = parcel.GeometryIn(matchingSrid)! })
                .Where(parcel => !OverlayNGRobust.Overlay(buildingGeometry, parcel.Geometry, SpatialFunction.Intersection).IsEmpty && parcel.Status == ParcelStatus.Realized)
                .Select(parcel =>
                    new {
                        parcel.CaPaKey,
                        Overlap = CalculateOverlap(buildingGeometry, parcel.Geometry)
                    })
                .ToList();

            return underlyingParcels
                .Where(parcel => parcel.Overlap >= 0.8 / underlyingParcels.Count)
                .Select(parcel => parcel.CaPaKey);
        }

        private static Geometry ToMatchingCrs(Geometry geometry, int matchingSrid)
            => matchingSrid == SystemReferenceId.SridLambert2008
                ? geometry.IsLambert08() ? geometry : geometry.EnsureLambert08(2)
                : geometry.IsLambert72() ? geometry : geometry.EnsureLambert72().RoundCoordinates(2);

        private static double CalculateOverlap(Geometry? buildingGeometry, Geometry parcel)
        {
            if (buildingGeometry is null)
            {
                return 0;
            }

            try
            {
                return buildingGeometry.Intersection(parcel).Area / buildingGeometry.Area;
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
