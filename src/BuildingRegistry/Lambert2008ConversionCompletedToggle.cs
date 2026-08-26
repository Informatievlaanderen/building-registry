namespace BuildingRegistry
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;

    /// <summary>
    /// Indicates that the Lambert 2008 (EPSG 3812) conversion is complete in every register this repository
    /// compares geometries against — address, parcel and building. Spatial matching is then done in
    /// <see cref="MatchingSrid"/>: the parcel column held in that system is compared against, and an incoming
    /// building geometry is brought to it.
    ///
    /// Distinct from <c>UseLambert2008EventStoreToggle</c>, which goes on when the conversion *begins* and
    /// decides which system incoming GML is normalized to. Both are enabled once the conversion is over.
    ///
    /// Enabling this early is inefficient rather than wrong — both parcel columns are populated and the
    /// incoming geometry is normalized either way — but it must not be enabled before the Lambert 2008
    /// column is fully populated, which <see cref="Lambert2008MatchingReadiness"/> enforces. See ADR 0006.
    /// </summary>
    public sealed class Lambert2008ConversionCompletedToggle
    {
        public bool FeatureEnabled { get; }

        public int MatchingSrid => FeatureEnabled
            ? SystemReferenceId.SridLambert2008
            : SystemReferenceId.SridLambert72;

        public Lambert2008ConversionCompletedToggle(bool featureEnabled) => FeatureEnabled = featureEnabled;
    }
}
