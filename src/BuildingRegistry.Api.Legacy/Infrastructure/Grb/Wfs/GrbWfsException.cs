namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb.Wfs
{
    using System;

    public class GrbWfsException : Exception
    {
        public GrbWfsException(string message)
            : this(new Exception(message)) { }

        public GrbWfsException(Exception innerException)
            : base("Failed to retrieve data from GRB WFS-service", innerException) { }
    }
}
