namespace BuildingRegistry
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// Remembers, per process, that a Lambert 2008 geometry column has been observed complete.
    ///
    /// Matching against a column that still has NULLs would silently skip those rows — a building would come
    /// back with no parcels and no error — which is the one real hazard of a manually thrown
    /// <see cref="Lambert2008ConversionCompletedToggle"/>. The check itself is a scan, and its expensive case
    /// is the normal one (proving there are no NULLs), so it is memoized here rather than run per request.
    ///
    /// Memoized **per subject**, not once. There is more than one such column — the parcel consumer's and
    /// the building projection's — and they fill on independent schedules. A single flag would let whichever
    /// probe ran first satisfy the other, which is the same silent pass the guard exists to prevent.
    ///
    /// A subject only ever goes from false to true: the conversion fills the column and nothing empties it.
    /// Registered as a singleton, so a process pays for each check once. See ADR 0006.
    /// </summary>
    public sealed class Lambert2008MatchingReadiness
    {
        /// <summary>The parcel consumer's <c>ParcelItemsWithCount.GeometryLambert2008</c>.</summary>
        public const string Parcels = "parcels";

        /// <summary>Projections.Legacy's <c>BuildingDetailsV2.SysGeometryLambert2008</c>.</summary>
        public const string Buildings = "buildings";

        private readonly ConcurrentDictionary<string, bool> _verified = new();

        /// <summary>
        /// Runs <paramref name="hasIncompleteGeometry"/> once per <paramref name="subject"/> and throws if it
        /// reports NULLs, so a premature flip stops loudly instead of quietly returning nothing. A no-op
        /// after that subject has passed once.
        ///
        /// For callers whose whole matching path is synchronous; an asynchronous one awaits
        /// <see cref="EnsureVerified(string,Func{Task{bool}})"/> instead, so neither has to block on the other.
        /// </summary>
        public void EnsureVerified(string subject, Func<bool> hasIncompleteGeometry)
        {
            if (_verified.ContainsKey(subject))
            {
                return;
            }

            Verify(subject, hasIncompleteGeometry());
        }

        /// <inheritdoc cref="EnsureVerified(string,Func{bool})"/>
        public async Task EnsureVerified(string subject, Func<Task<bool>> hasIncompleteGeometry)
        {
            if (_verified.ContainsKey(subject))
            {
                return;
            }

            Verify(subject, await hasIncompleteGeometry());
        }

        private void Verify(string subject, bool hasIncompleteGeometry)
        {
            if (hasIncompleteGeometry)
            {
                throw new InvalidOperationException(
                    $"Cannot match in Lambert 2008: some {subject} have no Lambert 2008 geometry yet, so they "
                    + "would be silently skipped. FeatureToggles:Lambert2008ConversionCompleted must not be "
                    + "enabled before the conversion has filled the column.");
            }

            _verified[subject] = true;
        }
    }
}
