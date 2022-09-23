namespace BuildingRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class PolygonIsInvalidException : BuildingRegistryException
    {
        public PolygonIsInvalidException()
        { }

        private PolygonIsInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
