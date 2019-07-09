using Be.Vlaanderen.Basisregisters.Crab;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;

namespace BuildingRegistry.Importer.TestClient
{
    class Program
    {
        private static readonly HttpListener Listener = new HttpListener();
        private static readonly string VbrConnectionString = ConfigurationManager.ConnectionStrings["Crab2Vbr"].ConnectionString;
        private static readonly string EventsConnectionString = ConfigurationManager.ConnectionStrings["Events"].ConnectionString;

        static void Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            Console.CancelKeyPress += (sender, eventArgs) => Listener.Close();

            var port = Properties.Settings.Default.PortToListen;

            Listener.Prefixes.Add($"http://+:{port}/grar/");
            Listener.Start();
            Console.WriteLine($"Listening on http://localhost:{port} ...");
            Console.WriteLine("Press CTRL + C to exit.");

            while (Listener.IsListening)
            {
                var context = Listener.GetContext();
                var idAsString = context.Request.QueryString.GetValues("id")?.FirstOrDefault();
                var fromAsString = context.Request.QueryString.GetValues("from")?.FirstOrDefault();

                if (!Enum.TryParse(context.Request.QueryString.GetValues("mode")?.FirstOrDefault(), true, out ImportMode mode))
                    mode = ImportMode.Init;
                
                try
                {
                    if (string.IsNullOrEmpty(idAsString))
                    {
                        WriteErrorResponse(context.Response, "Please specify an 'id' parameter");
                        continue;
                    }

                    var terrainObjectId = new CrabTerrainObjectId(Convert.ToInt32(idAsString));
                    var generator = new CommandGenerator(VbrConnectionString, true);
                    var settings = new SettingsBasedConfig();
                    var fromDate = DateTime.MinValue;
                    
                    if (mode == ImportMode.Update && !DateTime.TryParse(fromAsString, out fromDate))
                    {
                        WriteErrorResponse(context.Response,
                            string.IsNullOrWhiteSpace(fromAsString)
                                ? "Please specify 'from' parameter for an update"
                                : $"Cannot parse parameter 'from' with value '{fromAsString}'");

                        continue;
                    }

                    var commandProcessor = new CommandProcessorBuilder<int>(generator)
                        .UseApiProxyFactory(logger =>
                            new TestClientHttpApiProxyFactory(
                                logger,
                                settings,
                                fromDate,
                                terrainObjectId,
                                mode))
                        .UseProcessedKeysSet(new NonPersistentProcessedKeysSet<int>())
                        .UseCommandProcessorConfig(settings)
                        .UseDefaultSerializerSettingsForCrabImports()
                        .UseLoggerFactory(new LoggerFactory())
                        .Build();

                    commandProcessor.Run(null, settings);

                    var osloId = GetOsloId(terrainObjectId);
                    WriteResponse(context.Response, osloId);
                }
                catch (Exception exception)
                {
                    WriteErrorResponse(context.Response, exception.Message + Environment.NewLine + exception.StackTrace + exception.InnerException?.StackTrace);
                }
            }
        }

        private static void WriteResponse(HttpListenerResponse response, string text)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(text);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }

        private static void WriteErrorResponse(HttpListenerResponse response, string error)
        {
            response.StatusCode = 500;
            WriteResponse(response, error);
        }

        private static string GetOsloId(CrabTerrainObjectId crabTerrainObjectId)
        {
            var streamId = crabTerrainObjectId.CreateDeterministicId();
            using (var connection = new SqlConnection(EventsConnectionString))
            {
                return connection.QueryFirstOrDefault<string>(@"
select JSON_VALUE(JsonData, '$.osloId') as osloid from buildingregistry.Messages m
inner join BuildingRegistry.Streams s on m.StreamIdInternal = s.IdInternal
where s.IdOriginal = @streamId AND [Type] = 'BuildingOsloIdWasAssigned'",
                    new { streamId });
            }
        }
    }
}
