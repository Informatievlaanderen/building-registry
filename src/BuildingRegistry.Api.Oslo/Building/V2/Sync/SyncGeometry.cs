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
        /// Coordinates are persisted at centimetre precision, which is what the Lambert transform is accurate
        /// to, so a transformed geometry is rounded to that rather than carrying floating point noise.
        /// </summary>
        private const int TransformedCoordinateDecimals = 2;

        /// <summary>
        /// Reads a persisted geometry in the reference system its EWKB carries and puts it in the one the
        /// caller asked for through <see cref="ObjectCrs"/>. Only a geometry that has to move is transformed
        /// and rounded; one already in the requested system is passed through untouched, so a caller that
        /// does not ask for Lambert 2008 sees byte-for-byte what the feed emitted before. See ADR 0004.
        /// </summary>
        public static Geometry ToRequestedCrs(byte[] extendedWkbGeometry, int objectSrid)
        {
            var geometry = WKBReaderFactory.CreateForEwkb(extendedWkbGeometry).Read(extendedWkbGeometry);

            if (objectSrid == SystemReferenceId.SridLambert2008)
            {
                return geometry.IsLambert08()
                    ? geometry
                    : geometry.EnsureLambert08(TransformedCoordinateDecimals);
            }

            return geometry.IsLambert72()
                ? geometry
                : geometry.EnsureLambert72().RoundCoordinates(TransformedCoordinateDecimals);
        }
    }
}
