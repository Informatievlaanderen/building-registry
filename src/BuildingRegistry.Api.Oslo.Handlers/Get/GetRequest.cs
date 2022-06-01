namespace BuildingRegistry.Api.Oslo.Handlers.Get
{
    using Abstractions.Building.Responses;
    using Abstractions.Infrastructure.Grb;
    using Abstractions.Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public class GetRequest : IRequest<BuildingOsloResponse>
    {
        public LegacyContext Context { get; }
        public SyndicationContext SyndicationContext { get; }
        public IOptions<ResponseOptions> ResponseOptions { get; }
        public IGrbBuildingParcel GrbBuildingParcel { get; }
        public int PersistentLocalId { get; }

        public GetRequest(LegacyContext context, SyndicationContext syndicationContext, IOptions<ResponseOptions> responseOptions, IGrbBuildingParcel grbBuildingParcel, int persistentLocalId)
        {
            Context = context;
            SyndicationContext = syndicationContext;
            ResponseOptions = responseOptions;
            GrbBuildingParcel = grbBuildingParcel;
            PersistentLocalId = persistentLocalId;
        }
    }
}
