namespace BuildingRegistry
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;

    /// <summary>
    /// Indicates that GRB works in Lambert 2008 (EPSG 3812). Geometry sent to GRB's ANO API is brought to
    /// <see cref="GrbSrid"/> first, so what it receives does not depend on which reference system this
    /// register happens to persist.
    /// </summary>
    /// <remarks>
    /// A third toggle rather than a reading of one of the other two, because the three move independently
    /// and this register is expected to convert first:
    ///
    /// <list type="bullet">
    /// <item><c>UseLambert2008EventStoreToggle</c> — which system this register persists.</item>
    /// <item><see cref="Lambert2008ConversionCompletedToggle"/> — which system spatial matching compares in,
    /// once every register compared against has converted.</item>
    /// <item>this one — which system GRB expects, which is nothing to do with either.</item>
    /// </list>
    ///
    /// Off is the safe default and the current state: the ANO API is told about a building in Lambert 72
    /// whatever the event store holds. Getting this wrong is not silent in the usual way — the coordinates
    /// arrive ~500 km from where the building is, and it is GRB that has to notice. See ADR 0007.
    /// </remarks>
    public sealed class UseLambert2008GrbToggle
    {
        public bool FeatureEnabled { get; }

        public int GrbSrid => FeatureEnabled
            ? SystemReferenceId.SridLambert2008
            : SystemReferenceId.SridLambert72;

        public UseLambert2008GrbToggle(bool featureEnabled) => FeatureEnabled = featureEnabled;
    }
}
