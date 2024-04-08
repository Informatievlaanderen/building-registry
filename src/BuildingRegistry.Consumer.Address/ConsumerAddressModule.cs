namespace BuildingRegistry.Consumer.Address
{
    using System;
    using Autofac;
    using Building;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ConsumerAddressModule : Module
    {
        public ConsumerAddressModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger<ConsumerAddressModule>();
            var connectionString = configuration.GetConnectionString("ConsumerAddress");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, serviceLifetime, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            services.AddScoped<IAddresses, ConsumerAddressContext>();
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ServiceLifetime serviceLifetime,
            ILoggerFactory loggerFactory,
            string consumerProjectionsConnectionString)
        {
            services
                .AddDbContext<ConsumerAddressContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(consumerProjectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ConsumerAddressContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ConsumerAddressContext));
        }
    }
}
