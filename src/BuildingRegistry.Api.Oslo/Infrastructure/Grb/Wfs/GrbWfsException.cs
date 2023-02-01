namespace BuildingRegistry.Api.Oslo.Infrastructure.Grb.Wfs
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class GrbWfsException : Exception
    {
        public GrbWfsException(string message)
            : this(new Exception(message))
        { }

        public GrbWfsException(Exception innerException)
            : base("Failed to retrieve data from GRB WFS-service", innerException)
        { }

        private GrbWfsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
