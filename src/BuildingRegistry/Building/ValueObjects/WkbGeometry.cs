namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    public sealed class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        public const int SridLambert72 = SystemReferenceId.SridLambert72;

        [JsonConstructor]
        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        public static ExtendedWkbGeometry? CreateEWkb(byte[]? wkb)
        {
            if (wkb == null)
            {
                return null;
            }
            try
            {
                wkb.TryReadSrid(out var srid);
                var reader = WKBReaderFactory.CreateForEwkb(wkb);
                var geometry = reader.Read(wkb);
                geometry.SRID = srid;
                var wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
                return new ExtendedWkbGeometry(wkbWriter.Write(geometry));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
