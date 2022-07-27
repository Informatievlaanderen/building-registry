namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitStatusPreventsBuildingUnitRealizationException : BuildingRegistryException
    {
        public BuildingUnitStatusPreventsBuildingUnitRealizationException()
        { }

        private BuildingUnitStatusPreventsBuildingUnitRealizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
