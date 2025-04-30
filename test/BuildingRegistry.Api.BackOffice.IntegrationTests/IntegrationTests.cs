namespace BuildingRegistry.Api.BackOffice.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("/v2/gebouwen/acties/plannen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/realiseren", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/realisering", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/nietrealiseren", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/nietrealisering", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/inaanbouwplaatsen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/inaanbouwplaatsing", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/wijzigen/schetsgeometriepolygoon", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/verwijderen", "dv_gr_geschetstgebouw_beheer dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouwen/1/acties/verwijder-ingemeten", "dv_gr_ingemetengebouw_beheer dv_gr_ingemetengebouw_uitzonderingen")]
        [InlineData("/v2/gebouweenheden/acties/plannen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/realiseren", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/realisering", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/nietrealiseren", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/nietrealisering", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/opheffen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/opheffing", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/verwijdering", "dv_gr_geschetstgebouw_beheer dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouweenheden/1/acties/regulariseren", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/regularisatie", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/deregulariseren", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/deregularisatie", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/adreskoppelen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/adresontkoppelen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/positie", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/verwijderen", "dv_gr_geschetstgebouw_beheer dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouweenheden/1/acties/verplaatsen", "dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen")]
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
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/gebouwen/acties/plannen")]
        [InlineData("/v2/gebouwen/1/acties/realiseren")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/realisering")]
        [InlineData("/v2/gebouwen/1/acties/nietrealiseren")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/nietrealisering")]
        [InlineData("/v2/gebouwen/1/acties/inaanbouwplaatsen")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/inaanbouwplaatsing")]
        [InlineData("/v2/gebouwen/1/acties/wijzigen/schetsgeometriepolygoon")]
        [InlineData("/v2/gebouwen/1/acties/verwijderen")]
        [InlineData("/v2/gebouwen/1/acties/verwijder-ingemeten")]
        [InlineData("/v2/gebouweenheden/acties/plannen")]
        [InlineData("/v2/gebouweenheden/1/acties/realiseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/realisering")]
        [InlineData("/v2/gebouweenheden/1/acties/nietrealiseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/nietrealisering")]
        [InlineData("/v2/gebouweenheden/1/acties/opheffen")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/verwijdering")]
        [InlineData("/v2/gebouweenheden/1/acties/regulariseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/regularisatie")]
        [InlineData("/v2/gebouweenheden/1/acties/deregulariseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/deregularisatie")]
        [InlineData("/v2/gebouweenheden/1/acties/adreskoppelen")]
        [InlineData("/v2/gebouweenheden/1/acties/adresontkoppelen")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/positie")]
        [InlineData("/v2/gebouweenheden/1/acties/verwijderen")]
        [InlineData("/v2/gebouweenheden/1/acties/verplaatsen")]
        public async Task ReturnsUnauthorized(string endpoint)
        {
            var client = _fixture.TestServer.CreateClient();

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/gebouwen/acties/plannen")]
        [InlineData("/v2/gebouwen/1/acties/realiseren")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/realisering")]
        [InlineData("/v2/gebouwen/1/acties/nietrealiseren")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/nietrealisering")]
        [InlineData("/v2/gebouwen/1/acties/inaanbouwplaatsen")]
        [InlineData("/v2/gebouwen/1/acties/corrigeren/inaanbouwplaatsing")]
        [InlineData("/v2/gebouwen/1/acties/wijzigen/schetsgeometriepolygoon")]
        [InlineData("/v2/gebouwen/1/acties/verwijderen")]
        [InlineData("/v2/gebouwen/1/acties/verwijderen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouwen/1/acties/verwijderen", "dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouwen/1/acties/verwijder-ingemeten", "dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouwen/1/acties/verwijder-ingemeten", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/acties/plannen")]
        [InlineData("/v2/gebouweenheden/1/acties/realiseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/realisering")]
        [InlineData("/v2/gebouweenheden/1/acties/nietrealiseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/nietrealisering")]
        [InlineData("/v2/gebouweenheden/1/acties/opheffen")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/verwijdering", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/verwijdering", "dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouweenheden/1/acties/regulariseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/regularisatie")]
        [InlineData("/v2/gebouweenheden/1/acties/deregulariseren")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/deregularisatie")]
        [InlineData("/v2/gebouweenheden/1/acties/adreskoppelen")]
        [InlineData("/v2/gebouweenheden/1/acties/adresontkoppelen")]
        [InlineData("/v2/gebouweenheden/1/acties/corrigeren/positie")]
        [InlineData("/v2/gebouweenheden/1/acties/verwijderen")]
        [InlineData("/v2/gebouweenheden/1/acties/verwijderen", "dv_gr_geschetstgebouw_beheer")]
        [InlineData("/v2/gebouweenheden/1/acties/verwijderen", "dv_gr_geschetstgebouw_uitzonderingen")]
        [InlineData("/v2/gebouweenheden/1/acties/verplaatsen", "dv_gr_geschetstgebouw_beheer")]
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
