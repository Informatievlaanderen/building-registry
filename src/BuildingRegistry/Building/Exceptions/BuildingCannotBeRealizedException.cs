namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingCannotBeRealizedException : BuildingRegistryException
    {
        public BuildingCannotBeRealizedException(BuildingStatus status)
            : base($"Cannot realize building with status '{status}'.")
        { }

        private BuildingCannotBeRealizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
