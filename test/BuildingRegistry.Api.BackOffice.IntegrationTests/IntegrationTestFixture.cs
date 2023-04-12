namespace BuildingRegistry.Api.BackOffice.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.DockerUtilities;
    using IdentityModel;
    using IdentityModel.Client;
    using Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class IntegrationTestFixture : IAsyncLifetime
    {
        private string _clientId;
        private string _clientSecret;

        public TestServer TestServer { get; private set; }
        public SqlConnection SqlConnection { get; private set; }

        public async Task<string> GetAccessToken(string? requiredScopes = null)
        {
            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = "https://authenticatie-ti.vlaanderen.be/op/v1/token",
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes ?? string.Empty) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            return response.AccessToken;
        }

        public async Task InitializeAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _clientId = configuration.GetValue<string>("ClientId");
            _clientSecret = configuration.GetValue<string>("ClientSecret");

            using var _ = DockerComposer.Compose("sqlserver.yml", "building-integration-tests");
            await WaitForSqlServerToBecomeAvailable();

            await CreateDatabase();

            var hostBuilder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseStartup<Startup>()
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
                .UseTestServer();

            TestServer = new TestServer(hostBuilder);
        }

        private async Task WaitForSqlServerToBecomeAvailable()
        {
            foreach (var _ in Enumerable.Range(0, 60))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (await OpenConnection())
                {
                    break;
                }
            }
        }

        private async Task<bool> OpenConnection()
        {
            try
            {
                SqlConnection = new SqlConnection("Server=localhost,5435;User Id=sa;Password=Pass@word;database=master;TrustServerCertificate=True;");
                await SqlConnection.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task CreateDatabase()
        {
            var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'building-registry') BEGIN CREATE DATABASE [building-registry] END", SqlConnection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DisposeAsync()
        {
            await SqlConnection.DisposeAsync();
        }
    }
}
