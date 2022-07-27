namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitCannotBePlannedException : BuildingRegistryException
    {
        public BuildingUnitCannotBePlannedException()
        { }

        private BuildingUnitCannotBePlannedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
