namespace BuildingRegistry.AllStream
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class AllStreamId : ValueObject<AllStreamId>
    {
        public static readonly AllStreamId Instance = new();

        private readonly Guid _id = new("3ffc6ade-644a-4b68-91d8-ee068ebfe51d");

        private AllStreamId() { }

        protected override IEnumerable<object> Reflect()
        {
            yield return _id;
        }

        public override string ToString() => _id.ToString("D");
    }
}
