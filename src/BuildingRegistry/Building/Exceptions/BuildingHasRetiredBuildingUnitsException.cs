namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingHasRetiredBuildingUnitsException : BuildingRegistryException
    {
        public BuildingHasRetiredBuildingUnitsException()
        { }

        private BuildingHasRetiredBuildingUnitsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
