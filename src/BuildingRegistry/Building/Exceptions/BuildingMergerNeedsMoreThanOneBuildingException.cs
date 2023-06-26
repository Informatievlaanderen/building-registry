namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingMergerNeedsMoreThanOneBuildingException : BuildingRegistryException
    {
        public BuildingMergerNeedsMoreThanOneBuildingException()
        { }

        private BuildingMergerNeedsMoreThanOneBuildingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
