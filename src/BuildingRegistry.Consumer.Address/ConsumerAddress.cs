namespace BuildingRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class ConsumerAddress : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IIdempotentConsumer<ConsumerAddressContext> _kafkaIdemIdompotencyConsumer;
        private readonly ILogger<ConsumerAddress> _logger;

        public ConsumerAddress(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<ConsumerAddressContext> kafkaIdemIdompotencyConsumer)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _backOfficeContextFactory = backOfficeContextFactory;
            _loggerFactory = loggerFactory;
            _kafkaIdemIdompotencyConsumer = kafkaIdemIdompotencyConsumer;

            _logger = loggerFactory.CreateLogger<ConsumerAddress>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var addressKafkaProjection =
                new ConnectedProjector<ConsumerAddressContext>(
                    Resolve.WhenEqualToHandlerMessageType(new AddressKafkaProjection().Handlers));

            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(
                    new CommandHandlingKafkaProjection(_backOfficeContextFactory).Handlers));

            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);

            try
            {
                await _kafkaIdemIdompotencyConsumer.ConsumeContinuously(async (message, context) =>
                {
                    _logger.LogInformation("Handling next message");

                    await commandHandlingProjector.ProjectAsync(commandHandler, message, stoppingToken)
                        .ConfigureAwait(false);
                    await addressKafkaProjection.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    //CancellationToken.None to prevent halfway consumption
                    await context.SaveChangesAsync(CancellationToken.None);

                }, stoppingToken);
            }
            catch (Exception)
            {
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
