namespace BuildingRegistry.Api.BackOffice.Abstractions.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.IO.GML2;
    using ExtendedWkbGeometry = BuildingRegistry.Building.ExtendedWkbGeometry;

    public static class GmlHelpers
    {
        private static readonly WKBWriter WkbWriter = new WKBWriter() { Strict = false, HandleSRID = true };

        public static GMLReader CreateGmlReader() => CreateGmlReader(ExtendedWkbGeometry.SridLambert72);

        public static GMLReader CreateGmlReader(int srid) => GmlFactory.CreateGmlReader(srid);

        /// <summary>
        /// Reads a GML string using the reference system of its own srsName attribute.
        /// </summary>
        /// <exception cref="InvalidOperationException">When the srsName is missing or is not a supported reference system.</exception>
        public static Geometry ReadGeometry(this string gml)
        {
            if (!gml.TryReadSridGml(out var srid))
            {
                throw new InvalidOperationException("Unsupported or missing srsName in GML.");
            }

            return CreateGmlReader(srid).Read(gml);
        }

        /// <summary>
        /// Reads a GML geometry in the reference system its own srsName declares, and persists it as EWKB
        /// carrying that SRID - so the event store records which reference system a geometry is in rather
        /// than leaving every reader to infer it.
        /// </summary>
        /// <remarks>
        /// It deliberately does not consult <c>UseLambert2008EventStoreToggle</c>: the BackOffice API
        /// normalizes every incoming geometry to the event store's reference system before the SQS message
        /// is created (ADR 0003), so the srsName arriving here is already the right one.
        ///
        /// What it must not do is what it did before - force-set the SRID to Lambert 72 - which silently
        /// relabelled a Lambert 2008 geometry rather than rejecting it, persisting coordinates ~500 km from
        /// where the building is. An unsupported or missing srsName throws in <see cref="ReadGeometry"/>.
        /// See ADR 0006.
        /// </remarks>
        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
            => ExtendedWkbGeometry.Create(gml.ReadGeometry());
    }
}
