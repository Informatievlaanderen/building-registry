namespace BuildingRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;

    public class BuildingLastChangedListRunner : LastChangedListRunner
    {
        public const string Name = "BuildingLastChangedListRunner";

        public BuildingLastChangedListRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<BuildingLastChangedListRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new BuildingProjections(),
                new BuildingUnitProjections())
        { }
    }
}
