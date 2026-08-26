namespace BuildingRegistry
{
    using System;

    /// <summary>
    /// Remembers, per process, that the Lambert 2008 parcel geometry column has been observed complete.
    ///
    /// Matching against a column that still has NULLs would silently skip those parcels — a building would
    /// come back with no parcels and no error — which is the one real hazard of a manually thrown
    /// <see cref="Lambert2008ConversionCompletedToggle"/>. The check itself is a scan, and its expensive case
    /// is the normal one (proving there are no NULLs), so it is memoized here rather than run per request.
    ///
    /// The flag only ever goes from false to true: the conversion fills the column and nothing empties it.
    /// Registered as a singleton, so a process pays for the check once. See ADR 0006.
    /// </summary>
    public sealed class Lambert2008MatchingReadiness
    {
        private volatile bool _verified;

        /// <summary>
        /// Runs <paramref name="hasIncompleteGeometry"/> once and throws if it reports NULLs, so a premature
        /// flip stops loudly instead of quietly returning nothing. A no-op after it has passed once.
        /// </summary>
        public void EnsureVerified(Func<bool> hasIncompleteGeometry)
        {
            if (_verified)
            {
                return;
            }

            if (hasIncompleteGeometry())
            {
                throw new InvalidOperationException(
                    "Cannot match in Lambert 2008: some parcels have no Lambert 2008 geometry yet, so they "
                    + "would be silently skipped. FeatureToggles:Lambert2008ConversionCompleted must not be "
                    + "enabled before the parcel register's conversion has filled the column.");
            }

            _verified = true;
        }
    }
}
