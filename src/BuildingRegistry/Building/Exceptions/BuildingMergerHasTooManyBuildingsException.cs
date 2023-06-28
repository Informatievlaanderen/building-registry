namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingMergerHasTooManyBuildingsException : BuildingRegistryException
    {
        public const int MaxNumberOfBuildingsToMerge = 20;

        public BuildingMergerHasTooManyBuildingsException()
            : base($"A building merger can only merge up to {MaxNumberOfBuildingsToMerge} buildings.")
        { }

        private BuildingMergerHasTooManyBuildingsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
