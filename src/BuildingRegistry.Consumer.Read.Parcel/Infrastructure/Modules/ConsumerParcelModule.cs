namespace BuildingRegistry.Consumer.Read.Parcel.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Building;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ConsumerParcelModule : Module
    {
        public ConsumerParcelModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger<ConsumerParcelModule>();
            var connectionString = configuration.GetConnectionString("ConsumerParcel");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, serviceLifetime, loggerFactory, connectionString!);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            services.AddScoped<IParcels, ConsumerParcelContext>();
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ServiceLifetime serviceLifetime,
            ILoggerFactory loggerFactory,
            string consumerProjectionsConnectionString)
        {
            services
                .AddDbContext<ConsumerParcelContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(consumerProjectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadParcel, Schema.ConsumerReadParcel);
                        sqlServerOptions.UseNetTopologySuite();
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ConsumerParcelContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ConsumerParcelContext));
        }
    }
}
