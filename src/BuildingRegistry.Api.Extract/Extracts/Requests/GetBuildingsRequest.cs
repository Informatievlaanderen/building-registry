namespace BuildingRegistry.Api.Extract.Abstractions
{
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Projections.Extract;

    public record GetBuildingsRequest(ExtractContext Context) : IRequest<FileResult>;
}
