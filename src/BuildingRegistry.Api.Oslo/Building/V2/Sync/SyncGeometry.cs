namespace BuildingRegistry.Api.Oslo.Building.V2.Sync
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;
    // GrAr.Common.NetTopology is imported above and declares its own WKBReaderFactory, whose
    // CreateForEwkb throws on SRID-less EWKB. Alias ours, which falls back to Lambert 72. See ADR 0004.
    using WKBReaderFactory = BuildingRegistry.WKBReaderFactory;

    public static class SyncGeometry
    {
        /// <summary>
        /// The decimals a transformed position is rounded to. A position is a point, where rounding moves it
        /// by at most half a centimetre and nothing downstream measures it — the case the address registry
        /// rounds for. An outline is a polygon and is not rounded: there, rounding moves every vertex and so
        /// the area. See ADR 0004.
        /// </summary>
        private const int TransformedPositionDecimals = 2;

        /// <summary>
        /// A building outline in the reference system the caller asked for through <see cref="ObjectCrs"/>,
        /// transformed but not rounded.
        /// </summary>
        public static Geometry OutlineToRequestedCrs(byte[] extendedWkbGeometry, int objectSrid)
            => ToRequestedCrs(extendedWkbGeometry, objectSrid, roundingPrecision: null);

        /// <summary>
        /// A building unit position in that same system. Rounded, unlike the outline it sits inside, because
        /// it is a point.
        /// </summary>
        public static Geometry PositionToRequestedCrs(byte[] extendedWkbPosition, int objectSrid)
            => ToRequestedCrs(extendedWkbPosition, objectSrid, TransformedPositionDecimals);

        /// <summary>
        /// Reads a persisted geometry in the reference system its EWKB carries and puts it in the one the
        /// caller asked for. Only a geometry that has to move is transformed; one already in the requested
        /// system is passed through untouched, so a caller that does not ask for Lambert 2008 sees
        /// byte-for-byte what the feed emitted before. See ADR 0004.
        /// </summary>
        private static Geometry ToRequestedCrs(byte[] extendedWkbGeometry, int objectSrid, int? roundingPrecision)
        {
            var geometry = WKBReaderFactory.CreateForEwkb(extendedWkbGeometry).Read(extendedWkbGeometry);

            if (objectSrid == SystemReferenceId.SridLambert2008)
            {
                if (geometry.IsLambert08())
                {
                    return geometry;
                }

                return roundingPrecision.HasValue
                    ? geometry.EnsureLambert08(roundingPrecision.Value)
                    : geometry.EnsureLambert08();
            }

            if (geometry.IsLambert72())
            {
                return geometry;
            }

            var lambert72 = geometry.EnsureLambert72();

            return roundingPrecision.HasValue
                ? lambert72.RoundCoordinates(roundingPrecision.Value)
                : lambert72;
        }
    }
}
