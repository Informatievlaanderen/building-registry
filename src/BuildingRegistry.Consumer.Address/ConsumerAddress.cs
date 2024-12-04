namespace BuildingRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Building;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class ConsumerAddress : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;
        private readonly IBuildings _buildings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IIdempotentConsumer<ConsumerAddressContext> _kafkaIdemIdempotencyConsumer;
        private readonly ILogger<ConsumerAddress> _logger;

        public ConsumerAddress(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            IBuildings buildings,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<ConsumerAddressContext> kafkaIdemIdempotencyConsumer)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _backOfficeContextFactory = backOfficeContextFactory;
            _buildings = buildings;
            _loggerFactory = loggerFactory;
            _kafkaIdemIdempotencyConsumer = kafkaIdemIdempotencyConsumer;

            _logger = loggerFactory.CreateLogger<ConsumerAddress>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var addressProjection =
                new ConnectedProjector<ConsumerAddressContext>(
                    Resolve.WhenEqualToHandlerMessageType(new AddressKafkaProjection().Handlers));

            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(
                    new CommandHandlingKafkaProjection(_backOfficeContextFactory, _buildings).Handlers));

            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);

            try
            {
                await _kafkaIdemIdempotencyConsumer.ConsumeContinuously(async (message, consumerContext) =>
                {
                    await ConsumeHandler(
                        commandHandlingProjector,
                        addressProjection,
                        commandHandler,
                        message,
                        consumerContext);
                }, stoppingToken);
            }
            catch (Exception)
            {
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task ConsumeHandler(
            ConnectedProjector<CommandHandler> commandHandlingProjector,
            ConnectedProjector<ConsumerAddressContext> addressKafkaProjection,
            CommandHandler commandHandler,
            object message,
            ConsumerAddressContext context)
        {
            _logger.LogInformation("Handling next message");

            await commandHandlingProjector.ProjectAsync(commandHandler, message, CancellationToken.None).ConfigureAwait(false);
            await addressKafkaProjection.ProjectAsync(context, message, CancellationToken.None).ConfigureAwait(false);

            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
