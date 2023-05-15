namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class GrbIdempotencyException : DomainException
    {
        public GrbIdempotencyException()
        { }

        public GrbIdempotencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public GrbIdempotencyException(string message)
            : base(message)
        { }

        public GrbIdempotencyException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
