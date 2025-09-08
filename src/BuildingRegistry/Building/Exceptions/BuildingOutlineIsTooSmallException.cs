namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingOutlineIsTooSmallException : BuildingRegistryException
    {
        public BuildingOutlineIsTooSmallException()
        { }

        private BuildingOutlineIsTooSmallException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
