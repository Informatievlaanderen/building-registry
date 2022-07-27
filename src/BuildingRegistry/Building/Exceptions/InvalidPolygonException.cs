namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class InvalidPolygonException : BuildingRegistryException
    {
        public InvalidPolygonException()
        { }

        private InvalidPolygonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
