namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingStatusPreventsNotRealizeBuildingException : BuildingRegistryException
    {
        public BuildingStatusPreventsNotRealizeBuildingException()
        { }

        private BuildingStatusPreventsNotRealizeBuildingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
