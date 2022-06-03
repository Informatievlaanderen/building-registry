namespace BuildingRegistry.Api.Legacy.Abstractions.Infrastructure.Grb.Wfs
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Microsoft.AspNetCore.Http;

    public class GrbWfsExceptionHandler : DefaultExceptionHandler<GrbWfsException>
    {
        protected override ProblemDetails GetApiProblemFor(GrbWfsException exception)
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status500InternalServerError,
                Title = ProblemDetails.DefaultTitle,
                Detail = exception.Message,
                ProblemTypeUri = ProblemDetails.GetTypeUriFor(exception)
            };
    }
}
