namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingCannotBePlacedUnderConstructionException : BuildingRegistryException
    {
        public BuildingCannotBePlacedUnderConstructionException(BuildingStatus status)
            : base($"Cannot put building with status '{status}' under construction.")
        { }

        private BuildingCannotBePlacedUnderConstructionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
