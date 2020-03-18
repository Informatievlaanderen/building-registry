namespace BuildingRegistry.Importer.Console.TestClient
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Hosting;

    class Program
    {
        public static void Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            Run(args).Build().Run();
        }

        private static IWebHostBuilder Run(string[] args)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(
                    new ProgramOptions
                    {
                        Hosting =
                        {
                            HttpPort = 6009
                        },
                        Logging =
                        {
                            WriteTextToConsole = false,
                            WriteJsonToConsole = false
                        },
                        Runtime =
                        {
                            CommandLineArgs = args
                        }
                    });
    }
}
