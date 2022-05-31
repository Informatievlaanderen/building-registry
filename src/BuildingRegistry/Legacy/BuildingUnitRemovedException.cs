namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingUnitRemovedException : DomainException
    {
        public BuildingUnitRemovedException()
        {
        }

        public BuildingUnitRemovedException(string message) : base(message)
        {
        }

        public BuildingUnitRemovedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
