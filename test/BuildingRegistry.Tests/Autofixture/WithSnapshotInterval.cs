namespace BuildingRegistry.Tests.Autofixture
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;

    public class WithSnapshotInterval : ICustomization
    {
        private readonly int _interval;

        public WithSnapshotInterval(int interval = 1000)
        {
            _interval = interval;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(_interval));
        }
    }
}
