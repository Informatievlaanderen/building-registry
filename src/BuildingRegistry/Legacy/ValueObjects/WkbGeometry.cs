namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    public class WkbGeometry : ByteArrayValueObject<WkbGeometry>
    {
        public const int SridLambert72 = 31370;

        [JsonConstructor]
        public WkbGeometry([JsonProperty("value")] byte[] wkbBytes) : base(wkbBytes) { }

        public WkbGeometry(string wkbBytesHex) : base(wkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();
    }

    public class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        private static readonly WKBReader WkbReader = WKBReaderFactory.Create();

        [JsonConstructor]
        public ExtendedWkbGeometry([JsonProperty("value")] byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        public static ExtendedWkbGeometry CreateEWkb(byte[] wkb)
        {
            if (wkb == null)
                return null;
            try
            {
                var geometry = WkbReader.Read(wkb);
                geometry.SRID = WkbGeometry.SridLambert72;
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
