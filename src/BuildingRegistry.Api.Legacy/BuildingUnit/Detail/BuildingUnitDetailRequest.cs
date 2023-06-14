namespace BuildingRegistry.Api.Legacy.BuildingUnit.Detail
{
    using MediatR;

    public record GetBuildingUnitDetailRequest(int PersistentLocalId) : IRequest<BuildingUnitResponseWithEtag>;
}
