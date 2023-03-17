namespace BuildingRegistry.Api.Grb.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Xunit;

    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("/v2/uploads/job", Scopes.DvGrIngemetengebouwBeheer)]
        public async Task ReturnsSuccess(string endpoint, string requiredScopes)
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(requiredScopes));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/uploads/job")]
        public async Task ReturnsUnauthorized(string endpoint)
        {
            var client = _fixture.TestServer.CreateClient();

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/uploads/job")]
        [InlineData("/v2/uploads/job", "dv_gr_geschetstgebouw_beheer")]
        public async Task ReturnsForbidden(string endpoint, string scope = "")
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(scope));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
