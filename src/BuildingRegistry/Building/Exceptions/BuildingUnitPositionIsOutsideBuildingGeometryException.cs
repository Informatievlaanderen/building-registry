namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitPositionIsOutsideBuildingGeometryException : BuildingRegistryException
    {
        public BuildingUnitPositionIsOutsideBuildingGeometryException()
        { }

        private BuildingUnitPositionIsOutsideBuildingGeometryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
