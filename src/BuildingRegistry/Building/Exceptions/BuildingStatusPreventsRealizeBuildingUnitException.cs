namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingStatusPreventsBuildingUnitRealizationException : BuildingRegistryException
    {
        public BuildingStatusPreventsBuildingUnitRealizationException()
        { }

        private BuildingStatusPreventsBuildingUnitRealizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
