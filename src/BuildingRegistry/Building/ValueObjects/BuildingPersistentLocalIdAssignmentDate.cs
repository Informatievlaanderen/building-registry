namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    public class BuildingPersistentLocalIdAssignmentDate : InstantValueObject<BuildingPersistentLocalIdAssignmentDate>
    {
        public BuildingPersistentLocalIdAssignmentDate([JsonProperty("value")] Instant assignmentDate) : base(assignmentDate) { }
    }
}
