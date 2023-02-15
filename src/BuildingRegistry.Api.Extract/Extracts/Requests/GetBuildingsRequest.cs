namespace BuildingRegistry.Api.Extract.Extracts.Requests
{
    using BuildingRegistry.Projections.Extract;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public record GetBuildingsRequest(ExtractContext Context) : IRequest<FileResult>;
}
