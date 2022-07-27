namespace BuildingRegistry.Legacy
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class BuildingRemovedException : DomainException
    {
        public BuildingRemovedException()
        { }

        private BuildingRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public BuildingRemovedException(string message) : base(message)
        { }

        public BuildingRemovedException(string message, Exception inner) : base(message, inner)
        { }
    }
}
