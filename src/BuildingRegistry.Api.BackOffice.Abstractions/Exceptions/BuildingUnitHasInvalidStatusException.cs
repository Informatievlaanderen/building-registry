namespace BuildingRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitHasInvalidStatusException : BuildingRegistryException
    {
        public BuildingUnitHasInvalidStatusException()
        { }

        private BuildingUnitHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
