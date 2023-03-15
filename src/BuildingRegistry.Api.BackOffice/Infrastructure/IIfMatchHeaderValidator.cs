namespace BuildingRegistry.Api.BackOffice.Infrastructure
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.BuildingUnit.Extensions;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Building;

    public interface IIfMatchHeaderValidator
    {
        public Task<bool> IsValidForBuilding(string? ifMatchHeaderValue, BuildingPersistentLocalId buildingPersistentLocalId, CancellationToken ct);
        public Task<bool> IsValidForBuildingUnit(string? ifMatchHeaderValue, BuildingUnitPersistentLocalId buildingUnitPersistentLocalId, CancellationToken ct);
    }

    public class IfMatchHeaderValidator : IIfMatchHeaderValidator
    {
        private readonly IBuildings _buildings;
        private readonly BackOfficeContext _backOfficeContext;

        public IfMatchHeaderValidator(
            IBuildings buildings,
            BackOfficeContext backOfficeContext)
        {
            _buildings = buildings;
            _backOfficeContext = backOfficeContext;
        }

        public async Task<bool> IsValidForBuilding(
            string? ifMatchHeaderValue,
            BuildingPersistentLocalId buildingPersistentLocalId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(ifMatchHeaderValue))
            {
                return true;
            }

            var etag = await GetBuildingEtag(buildingPersistentLocalId, ct);

            return IfMatchHeaderMatchesEtag(ifMatchHeaderValue, etag);
        }

        public async Task<bool> IsValidForBuildingUnit(
            string? ifMatchHeaderValue,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(ifMatchHeaderValue))
            {
                return true;
            }

            var buildingPersistentLocalId = _backOfficeContext.GetBuildingIdForBuildingUnit(buildingUnitPersistentLocalId);

            var etag = await GetBuildingUnitEtag(buildingPersistentLocalId, buildingUnitPersistentLocalId, ct);

            return IfMatchHeaderMatchesEtag(ifMatchHeaderValue, etag);
        }

        private async Task<ETag> GetBuildingUnitEtag(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _buildings.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);

            var buildingUnit = aggregate.BuildingUnits.FirstOrDefault(
                x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new AggregateIdIsNotFoundException();
            }

            return new ETag(ETagType.Strong, buildingUnit.LastEventHash);
        }


        private async Task<ETag> GetBuildingEtag(
            int buildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _buildings.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);
            return new ETag(ETagType.Strong, aggregate.LastEventHash);
        }

        private bool IfMatchHeaderMatchesEtag(string ifMatchHeaderValue, ETag etag)
        {
            var ifMatchTag = ifMatchHeaderValue.Trim();
            return ifMatchTag == etag.ToString();
        }
    }
}
