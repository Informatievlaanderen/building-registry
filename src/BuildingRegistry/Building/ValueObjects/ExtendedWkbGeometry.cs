namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    public sealed class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        private static readonly WKBWriter WkbWriter = new WKBWriter { Strict = false, HandleSRID = true };

        public const int SridLambert72 = SystemReferenceId.SridLambert72;
        public const int SridLambert2008 = SystemReferenceId.SridLambert2008;

        [JsonConstructor]
        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        /// <summary>
        /// The EWKB as persisted. Readers take bytes, so this is what they get: going through
        /// <see cref="ToString"/> would allocate a hex string per geometry and parse it straight back.
        /// </summary>
        /// <remarks>
        /// A copy, not <c>Value</c> itself, which is <c>protected</c> on the base for a reason. This value
        /// object's equality components are its individual bytes — <c>ByteArrayValueObject.Reflect()</c>
        /// casts the array to <c>IEnumerable&lt;object&gt;</c> — so handing out the backing array would let
        /// a caller change what an already-constructed geometry equals and hashes to. The copy still keeps
        /// what this method exists for: no hex encode and decode per read.
        /// </remarks>
        public byte[] ToByteArray() => (byte[])Value.Clone();

        /// <summary>
        /// Wraps a geometry that has already been read and transformed, keeping the SRID it carries. Every
        /// path that writes a new or corrected building geometry goes through here, so this is what decides
        /// how one is serialized. <see cref="CreateEWkb"/> is the exception: it re-serializes a geometry the
        /// event store already holds, for a projection to read SRID-less legacy hex.
        /// </summary>
        /// <remarks>
        /// Deliberately does not round. A building outline or GRB measurement is a boundary whose vertices
        /// carry far more decimals than a centimetre, and rounding those would move it (ADR 0007). A
        /// building unit <em>position</em> is the other case and goes through
        /// <see cref="CreatePosition"/>.
        /// </remarks>
        public static ExtendedWkbGeometry Create(Geometry geometry)
            => new ExtendedWkbGeometry(WkbWriter.Write(geometry));

        /// <summary>
        /// A building unit position, rounded to
        /// <see cref="GeometryReferenceSystem.PositionRoundingPrecision"/> decimals — centimetres, the
        /// precision the event store holds positions at and the only one anything reads them back at.
        /// </summary>
        /// <remarks>
        /// The rounding belongs on the way in, not only in the Lambert 2008 transformation that already did
        /// it. Nothing downstream can reproduce a position finer than a centimetre: every reader rounds to
        /// two decimals, and <c>GmlGeometryNormalizer</c> passes a position already in the event store's
        /// reference system through verbatim, so without this a caller could persist millimetres that they
        /// would never be served back. See ADR 0007.
        ///
        /// The geometry is copied first: <c>RoundCoordinates</c> rounds in place, and callers hand us
        /// geometries they still use.
        /// </remarks>
        public static ExtendedWkbGeometry CreatePosition(Geometry geometry)
            => Create(geometry.Copy().RoundCoordinates(GeometryReferenceSystem.PositionRoundingPrecision));

        public static ExtendedWkbGeometry? CreateEWkb(byte[]? wkb, int useSrid = SridLambert72)
        {
            if (wkb == null)
            {
                return null;
            }

            try
            {
                if (!wkb.TryReadSrid(out var srid))
                {
                    if (useSrid == SridLambert72)
                    {
                        var geometry = WKBReaderFactory.CreateForLambert72().Read(wkb);
                        return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
                    }

                    if (useSrid == SystemReferenceId.SridLambert2008)
                    {
                        var geometry = WKBReaderFactory.CreateForLambert2008().Read(wkb);
                        return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
                    }

                    return null;
                }

                if (srid != useSrid)
                    throw new InvalidOperationException("SRID in EWKB does not match the expected SRID.");

                var reader = WKBReaderFactory.CreateForEwkb(wkb);
                var ewkbGeometry = reader.Read(wkb);
                ewkbGeometry.SRID = srid;

                return new ExtendedWkbGeometry(WkbWriter.Write(ewkbGeometry));
            }
            catch (ParseException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
