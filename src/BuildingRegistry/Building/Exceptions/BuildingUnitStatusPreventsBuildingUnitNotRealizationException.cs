namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitStatusPreventsBuildingUnitNotRealizationException : BuildingRegistryException
    {
        public BuildingUnitStatusPreventsBuildingUnitNotRealizationException()
        { }

        private BuildingUnitStatusPreventsBuildingUnitNotRealizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
