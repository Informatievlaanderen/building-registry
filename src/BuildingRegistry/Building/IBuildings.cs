namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IBuildings : IAsyncRepository<Building, BuildingStreamId> { }

    public class BuildingStreamId : ValueObject<BuildingStreamId>
    {
        private readonly BuildingPersistentLocalId _buildingPersistentLocalId;

        public BuildingStreamId(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return _buildingPersistentLocalId;
        }

        public override string ToString() => $"building-{_buildingPersistentLocalId}";
    }
}
