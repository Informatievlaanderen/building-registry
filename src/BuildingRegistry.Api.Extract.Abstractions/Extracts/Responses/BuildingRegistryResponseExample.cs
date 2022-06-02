namespace BuildingRegistry.Api.Extract.Abstractions.Extracts.Responses
{
    using Swashbuckle.AspNetCore.Filters;

    public class BuildingRegistryResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
            => new { mimeType = "application/zip", fileName = $"{ExtractFileNames.GetBuildingZipName()}.zip" };
    }
}
