namespace BuildingRegistry.Projector.Infrastructure
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Modules;
    using Serilog;
    using Serilog.Extensions.Logging;

    public class Program
    {
        protected Program()
        { }

        public static void Main(string[] args)
            => Run(new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = 6006
                    },
                    Logging =
                    {
                        WriteTextToConsole = false,
                        WriteJsonToConsole = false
                    },
                    Runtime =
                    {
                        CommandLineArgs = args
                    },
                    MiddlewareHooks =
                    {
                        ConfigureDistributedLock = DistributedLockOptions.LoadFromConfiguration
                    }
                });

        private static void Run(ProgramOptions options)
            => new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    builder.RegisterModule(new LoggingModule(hostContext.Configuration, services));
                    var logger = new SerilogLoggerFactory(Log.Logger);
                    builder.RegisterModule(new ApiModule(hostContext.Configuration, services, logger));
                })
                .UseDefaultForApi<Startup>(options)
                .RunWithLock<Program>();
    }
}
