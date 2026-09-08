namespace BuildingRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;

    /// <summary>
    /// What one building stream cost. Load and dispatch are kept apart because they scale with different
    /// things — loading with the number of events in the stream, dispatching with the geometry and the
    /// number of unit positions converted — and a staging run is only extrapolatable to production if you
    /// can tell which of the two dominates.
    /// </summary>
    internal sealed record StreamConversionResult(
        int BuildingUnitCount,
        int BuildingUnitsToConvert,
        bool GeometryToConvert,
        TimeSpan LoadDuration,
        TimeSpan DispatchDuration)
    {
        public TimeSpan TotalDuration => LoadDuration + DispatchDuration;

        /// <summary>Whether the stream held anything still in Lambert 72.</summary>
        public bool WasConverted => GeometryToConvert || BuildingUnitsToConvert > 0;

        public static StreamConversionResult Skipped { get; } = new(0, 0, false, TimeSpan.Zero, TimeSpan.Zero);
    }
}
