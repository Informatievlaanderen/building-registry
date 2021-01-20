namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;

    public class WfsException : Exception
    {
        public WfsException(string message)
            : this(new Exception(message))
        { }

        public WfsException(Exception innerException)
            : base("Failed to retrieve WFS data from GRB", innerException)
        { }
    }
}
