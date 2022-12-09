namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressIsRemovedException : BuildingRegistryException
    {
        public AddressIsRemovedException()
        { }

        private AddressIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
