namespace BuildingRegistry.Legacy
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class BuildingUnitRemovedException : DomainException
    {
        public BuildingUnitRemovedException()
        { }

        private BuildingUnitRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public BuildingUnitRemovedException(string message) : base(message)
        { }

        public BuildingUnitRemovedException(string message, Exception inner) : base(message, inner)
        { }
    }
}
