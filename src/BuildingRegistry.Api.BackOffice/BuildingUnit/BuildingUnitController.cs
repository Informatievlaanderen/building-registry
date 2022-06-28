namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "gebouweenheden")]
    public partial class BuildingUnitController : BuildingRegistryController
    {
        private readonly IMediator _mediator;
        private readonly IBuildings _buildingRepository;
        private readonly BackOfficeContext _backOfficeContext;

        public BuildingUnitController(
            IMediator mediator,
            IBuildings buildingsRepository,
            BackOfficeContext backOfficeContext)
            : base(mediator)
        {
            _mediator = mediator;
            _buildingRepository = buildingsRepository;
            _backOfficeContext = backOfficeContext;
        }

        protected bool IfMatchHeaderMatchesEtag(string ifMatchHeaderValue, ETag etag)
        {
            var ifMatchTag = ifMatchHeaderValue.Trim();
            return ifMatchTag == etag.ToString();
        }

        protected async Task<ETag> GetBuildingUnitEtag(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _buildingRepository.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);

            var buildingUnit = aggregate.BuildingUnits.FirstOrDefault(
                x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException();
            }

            return new ETag(ETagType.Strong, buildingUnit.LastEventHash);
        }

        protected bool TryGetBuildingIdForBuildingUnit(int buildingUnitPersistentLocalId, out int buildingPersistentLocalId)
        {
            buildingPersistentLocalId = 0;

            var buildingUnitBuilding = _backOfficeContext.BuildingUnitBuildings
                .FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnitBuilding is null)
            {
                return false;
            }

            buildingPersistentLocalId = buildingUnitBuilding.BuildingPersistentLocalId;
            return true;
        }
    }
}
