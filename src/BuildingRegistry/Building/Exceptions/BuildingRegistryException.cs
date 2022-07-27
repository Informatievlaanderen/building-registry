namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class BuildingRegistryException : DomainException
    {
        protected BuildingRegistryException()
        { }

        protected BuildingRegistryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
        
        protected BuildingRegistryException(string message)
            : base(message)
        { }

        protected BuildingRegistryException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
