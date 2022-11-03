namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using NodaTime;

    public sealed class BuildingPersistentLocalIdAssignmentDate : InstantValueObject<BuildingPersistentLocalIdAssignmentDate>
    {
        public BuildingPersistentLocalIdAssignmentDate(Instant assignmentDate) : base(assignmentDate) { }
    }
}
