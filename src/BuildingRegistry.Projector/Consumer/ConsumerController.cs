namespace BuildingRegistry.Projector.Consumer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using BuildingRegistry.Consumer.Address;
    using BuildingRegistry.Consumer.Read.Parcel;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("consumers")]
    public class ConsumersController : ApiController
    {
        private const string? ConsumerAddressConnectionStringKey = "ConsumerAddress";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            await using var sqlAddressConnection =
                new SqlConnection(configuration.GetConnectionString(ConsumerAddressConnectionStringKey));
            var addressResult =
                await sqlAddressConnection.QueryFirstAsync<DateTimeOffset>(
                    $"SELECT TOP(1) [{nameof(ProcessedMessage.DateProcessed)}] FROM [{Schema.ConsumerAddress}].[{ConsumerAddressContext.ProcessedMessageTable}] ORDER BY [{nameof(ProcessedMessage.DateProcessed)}] DESC");

            return Ok(new[]
            {
                new
                {
                    Name = "Consumer van adres",
                    LastProcessedMessage = addressResult
                },new
                {
                    Name = "Consumer van perceel (geen data)",
                    LastProcessedMessage = DateTimeOffset.Now
                }
            });
        }
    }
}
