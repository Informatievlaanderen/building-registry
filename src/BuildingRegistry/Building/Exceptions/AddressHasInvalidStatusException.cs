namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class AddressHasInvalidStatusException : BuildingRegistryException
    {
        public AddressHasInvalidStatusException()
        { }

        private AddressHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
