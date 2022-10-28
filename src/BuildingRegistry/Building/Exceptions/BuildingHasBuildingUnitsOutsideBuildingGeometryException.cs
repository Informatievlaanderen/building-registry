namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingHasBuildingUnitsOutsideBuildingGeometryException : BuildingRegistryException
    {
        public BuildingHasBuildingUnitsOutsideBuildingGeometryException()
        { }

        private BuildingHasBuildingUnitsOutsideBuildingGeometryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
