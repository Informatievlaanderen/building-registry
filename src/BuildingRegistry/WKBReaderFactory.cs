namespace BuildingRegistry
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.IO;
    using GrArWKBReaderFactory = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory;

    // ReSharper disable once InconsistentNaming
    public static class WKBReaderFactory
    {
        public static WKBReader CreateForLegacy() =>
            GrArWKBReaderFactory.CreateForLambert72();

        public static WKBReader Create() =>
            GrArWKBReaderFactory.CreateForLambert72();

        public static WKBReader CreateForLambert2008() =>
            GrArWKBReaderFactory.CreateForLambert2008();

        /// <summary>
        /// Creates a reader for a persisted geometry, in the reference system the bytes themselves carry,
        /// so callers do not have to assume which one the event store writes.
        /// Geometries persisted before the event store recorded an SRID are read as Lambert 72.
        /// See ADR 0004.
        /// </summary>
        /// <remarks>
        /// This shares its name with <c>Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory</c>,
        /// whose <c>CreateForEwkb</c> throws on SRID-less EWKB where this one falls back. A file that imports
        /// that namespace binds the simple name to GrAr's, silently — so pin it with a using-alias there.
        /// </remarks>
        public static WKBReader CreateForEwkb(byte[] ewkb) =>
            ewkb.TryReadSrid(out _)
                ? GrArWKBReaderFactory.CreateForEwkb(ewkb)
                : GrArWKBReaderFactory.CreateForLambert72();
    }
}
