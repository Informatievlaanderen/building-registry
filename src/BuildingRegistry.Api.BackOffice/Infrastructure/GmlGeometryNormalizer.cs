namespace BuildingRegistry.Api.BackOffice.Infrastructure
{
    using Abstractions;
    using Abstractions.Building;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;

    /// <summary>
    /// Converts an incoming GML geometry into the reference system the event store persists,
    /// so that everything downstream (SQS message, lambda, aggregate) only ever sees a single reference system.
    /// </summary>
    public sealed class GmlGeometryNormalizer
    {
        private readonly UseLambert2008EventStoreToggle _useLambert2008EventStore;

        public GmlGeometryNormalizer(UseLambert2008EventStoreToggle useLambert2008EventStore)
        {
            _useLambert2008EventStore = useLambert2008EventStore;
        }

        /// <returns>
        /// The GML unchanged when it is already in the event store's reference system, the converted GML otherwise.
        /// </returns>
        public string ToEventStoreSrs(string gml)
        {
            var geometry = gml.ReadGeometry();
            var eventStoreSrid = _useLambert2008EventStore.EventStoreSrid;

            if (geometry.SRID == eventStoreSrid)
            {
                return gml;
            }

            var converted = eventStoreSrid == SystemReferenceId.SridLambert2008
                ? geometry.TransformFromLambert72To08()
                : geometry.TransformFromLambert08To72();

            // http srsName, matching SystemReferenceId and GrAr's own GML producers.
            return converted.ConvertToGml(false);
        }

        /// <summary>
        /// <see cref="ToEventStoreSrs"/> for an optional geometry — a position that was not supplied is passed through.
        /// </summary>
        public string? ToEventStoreSrsWhenPresent(string? gml) =>
            string.IsNullOrWhiteSpace(gml) ? gml : ToEventStoreSrs(gml);
    }
}
