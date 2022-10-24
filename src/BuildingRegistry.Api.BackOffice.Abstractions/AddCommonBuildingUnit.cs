namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    using BuildingRegistry.Building;

    public class AddCommonBuildingUnit : IAddCommonBuildingUnit
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public AddCommonBuildingUnit(
            BackOfficeContext backOfficeContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator)
        {
            _backOfficeContext = backOfficeContext;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        public int GenerateNextPersistentLocalId() => _persistentLocalIdGenerator.GenerateNextPersistentLocalId();

        public void AddForBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            _backOfficeContext.BuildingUnitBuildings.Add(new BuildingUnitBuilding(buildingUnitPersistentLocalId, buildingPersistentLocalId));
            _backOfficeContext.SaveChanges();
        }
    }
}
