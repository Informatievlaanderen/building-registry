namespace BuildingRegistry.Legacy
{
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Newtonsoft.Json;

    public class BuildingUnitId : GuidValueObject<BuildingUnitId>
    {
        private static readonly Guid Namespace = new Guid("50609103-0399-4717-b8e9-e13986bc5370");

        public BuildingUnitId([JsonProperty("value")] Guid buildingUnitId) : base(buildingUnitId)
        { }

        public static BuildingUnitId Create(BuildingUnitKey buildingUnitKey, int version)
            => new BuildingUnitId(Deterministic.Create(Namespace, BuildDeterministicString(buildingUnitKey, version)));

        private static string BuildDeterministicString(
            BuildingUnitKey buildingUnitKey,
            int version)
        {
            var stringBuilder = new StringBuilder($"BuildingUnitKey-{buildingUnitKey}");
            stringBuilder.Append($"Version-{version}");

            return stringBuilder.ToString();
        }
    }
}
