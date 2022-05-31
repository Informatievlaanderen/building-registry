namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingRemovedException : DomainException
    {
        public BuildingRemovedException()
        {
        }

        public BuildingRemovedException(string message) : base(message)
        {
        }

        public BuildingRemovedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
