namespace BuildingRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitIsRemovedException : BuildingRegistryException
    {
        public BuildingUnitIsRemovedException(int persistentLocalId)
            : base($"BuildingUnit with Id '{persistentLocalId}' is removed.")
        { }

        private BuildingUnitIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
