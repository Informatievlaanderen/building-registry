namespace BuildingRegistry.Api.Legacy.Infrastructure.ParcelMatching.Wfs
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Microsoft.AspNetCore.Http;

    public class GrbWfsExceptionHandler : DefaultExceptionHandler<GrbWfsException>
    {
        protected override ProblemDetails GetApiProblemFor(HttpContext context, GrbWfsException exception)
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status500InternalServerError,
                Title = ProblemDetails.DefaultTitle,
                Detail = exception.Message,
                ProblemTypeUri = ProblemDetails.GetTypeUriFor(exception)
            };
    }
}
