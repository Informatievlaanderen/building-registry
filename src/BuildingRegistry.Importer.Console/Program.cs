namespace BuildingRegistry.Importer.Console
{
    using System.IO;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Serilog;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Diagnostics;
    using System.Reflection;

    internal class Program
    {
        private static Stopwatch _stopwatch;
        private static int _commandCounter;

        private static void Main(params string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            var crabConnectionString = configuration.GetConnectionString("CRABEntities");
            Func<CRABEntities> crabEntitiesFactory = () =>
            {
                var factory = new CRABEntities(crabConnectionString);
                factory.Database.CommandTimeout = 60 * 10;
                return factory;
            };

            var settings = new SettingsBasedConfig(configuration.GetSection("ApplicationSettings"));
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                var options = new ImportOptions(
                    args,
                    errors => WaitForExit(settings.WaitForUserInput, "Could not parse commandline options."));

                MapLogging.Log = s => _commandCounter++;

                var commandProcessor = new CommandProcessorBuilder<int>(new CommandGenerator(configuration.GetConnectionString("Crab2Vbr"), crabEntitiesFactory))
                    .WithCommandLineOptions(options.ImportArguments)
                    .UseSerilog(cfg => cfg
                        .WriteTo.File(
                            "tracing.log",
                            LogEventLevel.Verbose,
                            retainedFileCountLimit: 20,
                            fileSizeLimitBytes: 104857600,
                            rollOnFileSizeLimit: true,
                            rollingInterval: RollingInterval.Day)
                        .WriteTo.Console(LogEventLevel.Information))
                    .UseHttpApiProxyConfig(settings)
                    .UseCommandProcessorConfig(settings)
                    .UseDefaultSerializerSettingsForCrabImports()
                    .ConfigureImportFeedFromAssembly(Assembly.GetExecutingAssembly())
                    .Build();

                WaitForStart(settings.WaitForUserInput);

                commandProcessor.Run(options, settings);

                WaitForExit(settings.WaitForUserInput);
            }
            catch (Exception exception)
            {
                WaitForExit(settings.WaitForUserInput, "General error occurred", exception);
            }
        }

        private static void WaitForExit(bool waitForUserInput, string errorMessage = null, Exception exception = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
                Console.Error.WriteLine(errorMessage);

            if (exception != null)
                Console.Error.WriteLine(exception);

            Console.WriteLine();

            if (_stopwatch != null)
            {
                var avg = _commandCounter / _stopwatch.Elapsed.TotalSeconds;
                var summary = $"Report: generated {_commandCounter} commands in {_stopwatch.Elapsed}ms (={avg}/second).";
                Console.WriteLine(summary);
            }

            if (waitForUserInput)
            {
                Console.WriteLine("Done! Press ENTER key to exit...");
                ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            }

            if (!string.IsNullOrEmpty(errorMessage))
                Environment.Exit(1);

            Environment.Exit(0);
        }

        private static void WaitForStart(bool waitForUserInput)
        {
            if (waitForUserInput)
            {
                Console.WriteLine("Press ENTER key to start the CRAB Import...");
                ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            }
            else
                Console.WriteLine("Starting CRAB Import...");

            _stopwatch = Stopwatch.StartNew();
        }
    }
}
