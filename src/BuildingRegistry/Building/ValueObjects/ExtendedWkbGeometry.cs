namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
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
        /// Wraps a geometry that has already been read and transformed, keeping the SRID it carries. The
        /// EWKB writer lives here, so this is the only place that decides how a geometry is serialized.
        /// </summary>
        public static ExtendedWkbGeometry Create(Geometry geometry)
            => new ExtendedWkbGeometry(WkbWriter.Write(geometry));

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
