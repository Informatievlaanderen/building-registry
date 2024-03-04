namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.OrWegwijsApi
{
    using System.Net.Http.Json;
    using System.Text.Json;

    public interface IWegwijsApiProxy
    {
        Task<string> GetOrganisationName(string ovoCode);
    }

    public class WegwijsApiProxy : IWegwijsApiProxy
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WegwijsApiProxy(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetOrganisationName(string ovoCode)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var uri = $"https://api.wegwijs.vlaanderen.be/v1/search/organisations?q=ovoNumber:{ovoCode}&sort=changeId&fields=name";

            var response = await httpClient.GetFromJsonAsync<WegWijsResponseItem[]>(
                uri,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return response is { Length: 1 } ? response.Single().Name : ovoCode;
        }
    }

    public record WegWijsResponseItem(string Name);
}
