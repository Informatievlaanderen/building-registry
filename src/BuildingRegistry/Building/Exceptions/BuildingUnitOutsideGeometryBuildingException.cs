namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitOutsideGeometryBuildingException : BuildingRegistryException
    {
        public BuildingUnitOutsideGeometryBuildingException()
        { }

        private BuildingUnitOutsideGeometryBuildingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
