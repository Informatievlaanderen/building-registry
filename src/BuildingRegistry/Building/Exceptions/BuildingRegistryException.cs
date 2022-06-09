namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class BuildingRegistryException : DomainException
    {
        protected BuildingRegistryException() { }

        protected BuildingRegistryException(string message) : base(message) { }

        protected BuildingRegistryException(string message, Exception inner) : base(message, inner) { }
    }
}
