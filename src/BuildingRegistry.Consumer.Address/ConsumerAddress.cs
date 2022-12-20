namespace BuildingRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class ConsumerAddress : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<ConsumerAddressContext> _dbContextFactory;
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IKafkaIdompotencyConsumer<ConsumerAddressContext> _kafkaIdemIdompotencyConsumer;
        private readonly ILogger<ConsumerAddress> _logger;

        public ConsumerAddress(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<ConsumerAddressContext> dbContextFactory,
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            ILoggerFactory loggerFactory,
            IKafkaIdompotencyConsumer<ConsumerAddressContext> kafkaIdemIdompotencyConsumer)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _dbContextFactory = dbContextFactory;
            _backOfficeContextFactory = backOfficeContextFactory;
            _loggerFactory = loggerFactory;
            _kafkaIdemIdompotencyConsumer = kafkaIdemIdompotencyConsumer;

            _logger = loggerFactory.CreateLogger<ConsumerAddress>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ValidateConsumerOffset(stoppingToken);

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

                    await commandHandlingProjector.ProjectAsync(commandHandler, message, stoppingToken).ConfigureAwait(false);
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

        private async Task ValidateConsumerOffset(CancellationToken cancellationToken)
        {
            if (_kafkaIdemIdompotencyConsumer.ConsumerOptions.Offset is not null)
            {
                await using (var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken))
                {
                    if (await context.AddressConsumerItems.AnyAsync(cancellationToken))
                    {
                        throw new InvalidOperationException(
                            "Cannot start consumer from offset, because consumer context already has data. Remove offset or clear data to continue.");
                    }
                }

                _logger.LogInformation($"{nameof(ConsumerAddress)} starting {_kafkaIdemIdompotencyConsumer.ConsumerOptions.Topic} from offset {_kafkaIdemIdompotencyConsumer.ConsumerOptions.Offset.Value}.");
            }

            _logger.LogInformation($"{nameof(ConsumerAddress)} continuing {_kafkaIdemIdompotencyConsumer.ConsumerOptions.Topic} from last offset.");
        }
    }
}
