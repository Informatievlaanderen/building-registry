namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Building;
    using IdentityModel;
    using IdentityModel.Client;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    public interface IAnoApiProxy
    {
        Task CreateAnomaly(
            int buildingPersistentLocalId,
            DateTimeOffset dateTimeStatusChange,
            string organisation,
            ExtendedWkbGeometry geometry,
            CancellationToken ct);
    }

    public class AnoApiProxy : IAnoApiProxy
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AnoApiOptions _options;

        public AnoApiProxy(
            IHttpClientFactory httpClientFactory,
            AnoApiOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public async Task CreateAnomaly(
            int buildingPersistentLocalId,
            DateTimeOffset dateTimeStatusChange,
            string organisation,
            ExtendedWkbGeometry geometry,
            CancellationToken ct)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken(ct));

            var anoRequest = new AnoRequest(
                buildingPersistentLocalId,
                organisation,
                dateTimeStatusChange,
                geometry);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await httpClient.PostAsJsonAsync(
                new Uri(_options.BaseUrl + "/api/processing/anomalies"),
                anoRequest,
                jsonSerializerOptions,
                ct);
        }

        private async Task<string> GetAccessToken(CancellationToken cancellationToken)
        {
            var tokenClient = new TokenClient(
                () => _httpClientFactory.CreateClient(),
                new TokenClientOptions
                {
                    Address = _options.TokenUrl,
                    ClientId = _options.TokenClientId,
                    ClientSecret = _options.TokenClientSecret,
                    Parameters = new Parameters(new[]
                    {
                        new KeyValuePair<string, string>("scope", "GRB_operator Gecko_datavalidator")
                    })
                });

            var tokenResponse = await tokenClient.RequestTokenAsync(
                OidcConstants.GrantTypes.ClientCredentials,
                cancellationToken: cancellationToken);

            return tokenResponse.AccessToken;
        }
    }

    internal class AnoRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "FeatureCollection";

        [JsonPropertyName("features")]
        public Feature[] Features { get; }

        public AnoRequest(
            int buildingPersistentLocalId,
            string organisation,
            DateTimeOffset dateTimeStatusChange,
            ExtendedWkbGeometry geometry)
        {
            Features = new[] { new Feature(buildingPersistentLocalId, organisation, dateTimeStatusChange, geometry) };
        }
    }

    internal class Feature
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "Feature";

        [JsonPropertyName("geometry")]
        public GeoJSONPolygon Geometry { get; }

        [JsonPropertyName("properties")]
        public Properties Properties { get; }

        public Feature(int buildingPersistentLocalId, string organisation, DateTimeOffset dateTimeStatusChange, ExtendedWkbGeometry geometry)
        {
            Geometry = MapToGeoJsonPolygon((Polygon) WKBReaderFactory.Create().Read(geometry));
            Properties = new Properties(buildingPersistentLocalId, organisation, dateTimeStatusChange);
        }

        private static GeoJSONPolygon MapToGeoJsonPolygon(Polygon polygon)
        {
            var rings = polygon.InteriorRings.ToList();
            rings.Insert(0, polygon.ExteriorRing); //insert exterior ring as first item

            var output = new double[rings.Count][][];
            for (var i = 0; i < rings.Count; i++)
            {
                output[i] = new double[rings[i].Coordinates.Length][];

                for (var j = 0; j < rings[i].Coordinates.Length; j++)
                {
                    output[i][j] = new double[2];
                    output[i][j][0] = rings[i].Coordinates[j].X;
                    output[i][j][1] = rings[i].Coordinates[j].Y;
                }
            }

            return new GeoJSONPolygon { Coordinates = output };
        }
    }

    internal class Properties
    {
        [JsonPropertyName("GVC")]
        public int GVC { get; } = 1;

        [JsonPropertyName("GVS")]
        public string GVS { get; }

        [JsonPropertyName("GROIDA")]
        public string GROIDA { get; }

        [JsonPropertyName("GVCASB")]
        public int GVCASB { get; } = 2;

        [JsonPropertyName("TPC")]
        public int TPC { get; } = 2;

        [JsonPropertyName("TPCV")]
        public int TPCV { get; } = -9;

        [JsonPropertyName("GVDOPM")]
        public string GVDOPM { get; }

        [JsonPropertyName("SHAPE")]
        public object SHAPE { get; } = "polygoon";

        [JsonPropertyName("INVCB")]
        public int INVCB { get; } = 4;

        [JsonPropertyName("EXNIA")]
        public string EXNIA { get; } = "nvt";

        [JsonPropertyName("EXNWO")]
        public int EXNWO { get; } = -9;

        [JsonPropertyName("GVCO")]
        public int GVCO { get; } = 1;

        [JsonPropertyName("GVSB")]
        public string GVSB { get; } = "nvt";

        public Properties(
            int buildingPersistentLocalId,
            string organisation,
            DateTimeOffset dateTimeStatusChange)
        {
            GVS = $"GR gebouw gerealiseerd | {buildingPersistentLocalId}";
            GROIDA = organisation;
            GVDOPM = $"{dateTimeStatusChange:O}Z";
        }
    }
}
