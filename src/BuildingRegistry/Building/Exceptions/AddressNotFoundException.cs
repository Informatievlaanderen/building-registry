namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressNotFoundException : BuildingRegistryException
    {
        public AddressNotFoundException()
        { }

        private AddressNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
