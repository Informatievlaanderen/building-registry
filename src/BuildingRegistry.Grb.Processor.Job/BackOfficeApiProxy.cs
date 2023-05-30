namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Newtonsoft.Json;
    using Polly;

    public interface IBackOfficeApiProxy
    {
        Task<BackOfficeApiResult> RealizeAndMeasureUnplannedBuilding(
            RealizeAndMeasureUnplannedBuildingRequest request,
            CancellationToken cancellationToken);
        Task<BackOfficeApiResult> DemolishBuilding(
            int buildingPersistentLocalId,
            DemolishBuildingRequest request,
            CancellationToken cancellationToken);
        Task<BackOfficeApiResult> MeasureBuilding(
            int buildingPersistentLocalId,
            MeasureBuildingRequest request,
            CancellationToken cancellationToken);
        Task<BackOfficeApiResult> ChangeBuildingMeasurement(
            int buildingPersistentLocalId,
            ChangeBuildingMeasurementRequest request,
            CancellationToken cancellationToken);
        Task<BackOfficeApiResult> CorrectBuildingMeasurement(
            int buildingPersistentLocalId,
            CorrectBuildingMeasurementRequest request,
            CancellationToken cancellationToken);
    }

    public sealed class BackOfficeApiProxy : IBackOfficeApiProxy
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BackOfficeApiProxy(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BackOfficeApiResult> RealizeAndMeasureUnplannedBuilding(
            RealizeAndMeasureUnplannedBuildingRequest request,
            CancellationToken cancellationToken)
        {
            return await Execute(request, "v2/gebouwen/acties/vaststellen", cancellationToken);
        }

        public async Task<BackOfficeApiResult> DemolishBuilding(
            int buildingPersistentLocalId,
            DemolishBuildingRequest request,
            CancellationToken cancellationToken)
        {
            return await Execute(request, $"v2/gebouwen/{buildingPersistentLocalId}/acties/slopen", cancellationToken);
        }

        public async  Task<BackOfficeApiResult> MeasureBuilding(
            int buildingPersistentLocalId,
            MeasureBuildingRequest request,
            CancellationToken cancellationToken)
        {
            return await Execute(request, $"v2/gebouwen/{buildingPersistentLocalId}/acties/inmeten", cancellationToken);
        }

        public async Task<BackOfficeApiResult> ChangeBuildingMeasurement(
            int buildingPersistentLocalId,
            ChangeBuildingMeasurementRequest request,
            CancellationToken cancellationToken)
        {
            return await Execute(request, $"v2/gebouwen/{buildingPersistentLocalId}/acties/wijzigen/ingemetengeometriepolygoon", cancellationToken);
        }

        public async  Task<BackOfficeApiResult> CorrectBuildingMeasurement(
            int buildingPersistentLocalId,
            CorrectBuildingMeasurementRequest request,
            CancellationToken cancellationToken)
        {
            return await Execute(request, $"v2/gebouwen/{buildingPersistentLocalId}/acties/corrigeren/ingemetengeometriepolygoon", cancellationToken);
        }

        private async Task<BackOfficeApiResult> Execute<TRequest>(
            TRequest request,
            string route,
            CancellationToken cancellationToken)
        {
            try
            {
                return await Policy
                    .Handle<HttpRequestException>(x => x.StatusCode == HttpStatusCode.InternalServerError)
                    .WaitAndRetryAsync(10, currentRetry => Math.Pow(currentRetry, 2) * TimeSpan.FromSeconds(2))
                    .ExecuteAsync(async () =>
                    {
                        using var httpClient = _httpClientFactory.CreateClient(nameof(BackOfficeApiProxy));

                        var content = new StringContent(
                            JsonConvert.SerializeObject(request),
                            Encoding.UTF8,
                            "application/json");

                        var response = await httpClient.PostAsync(route, content, cancellationToken);

                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                            var validationProblemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(jsonResponse);

                            var validationErrors =
                                validationProblemDetails.ValidationErrors.SelectMany(x => x.Value).ToList();

                            return new BackOfficeApiResult(null, validationErrors);
                        }

                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                            var validationProblemDetails = JsonConvert.DeserializeObject<ProblemDetails>(jsonResponse);

                            return new BackOfficeApiResult(null, new[] { new ValidationError(validationProblemDetails.Detail) });
                        }

                        response.EnsureSuccessStatusCode();

                        var ticketUrl = response.Headers.Location;
                        return new BackOfficeApiResult(ticketUrl.ToString(), null);
                    });
            }
            catch (HttpRequestException exception)
            {
                return new BackOfficeApiResult(null,
                    new[] { new ValidationError($"Http request failed with status code {exception.StatusCode}") });
            }
        }
    }

    public record BackOfficeApiResult(string? TicketUrl, IEnumerable<ValidationError>? ValidationErrors)
    {
        public bool IsSuccess => ValidationErrors is null || !ValidationErrors.Any();
    }
}
