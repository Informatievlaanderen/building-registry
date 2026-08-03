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

        // TODO: the lambda still assumes the event store persists Lambert 72. Make this SRID aware
        // when the event store is migrated to Lambert 2008 (see UseLambert2008EventStoreToggle).
        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return ExtendedWkbGeometry.CreateEWkb(WkbWriter.Write(geometry));
        }
    }
}
