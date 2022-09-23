namespace BuildingRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingHasInvalidStatusException : BuildingRegistryException
    {
        public BuildingHasInvalidStatusException()
        { }

        private BuildingHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
