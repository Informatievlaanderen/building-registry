namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;
    using System.Runtime.Serialization;

    public class WfsException : Exception
    {
        public WfsException(string message)
            : base(message)
        { }

        public WfsException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected WfsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
