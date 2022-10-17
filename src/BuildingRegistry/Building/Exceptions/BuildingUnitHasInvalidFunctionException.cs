namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitHasInvalidFunctionException : BuildingRegistryException
    {
        public BuildingUnitHasInvalidFunctionException()
        { }

        private BuildingUnitHasInvalidFunctionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
