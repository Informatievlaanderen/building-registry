namespace BuildingRegistry.Api.Extract.Extracts.Responses
{
    using Swashbuckle.AspNetCore.Filters;
    using System;

    public class BuildingRegistryResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
            => new { mimeType = "application/zip", fileName = $"{ExtractController.BuildingZipName}-{DateTime.Now:yyyy-MM-dd}.zip" };
    }
}
