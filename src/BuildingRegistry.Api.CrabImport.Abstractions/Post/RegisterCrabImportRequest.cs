namespace BuildingRegistry.Api.CrabImport.Abstractions.Post
{
    using System.ComponentModel.DataAnnotations;
    using Legacy;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class RegisterCrabImportRequest
    {
        /// <summary>Type van het CRAB item.</summary>
        [Required]
        public string Type { get; set; }

        /// <summary>Het CRAB item.</summary>
        [Required]
        public string CrabItem { get; set; }
    }

    public class RegisterCrabImportRequestExample : IExamplesProvider<RegisterCrabImportRequest>
    {
        public RegisterCrabImportRequest GetExamples()
            => new RegisterCrabImportRequest
            {
                Type = "BuildingRegistry.Municipality.Commands.ImportMunicipalityNameFromCrab",
                CrabItem = "{}"
            };
    }

    public static class RegisterCrabImportRequestMapping
    {
        public static dynamic? Map(RegisterCrabImportRequest message)
        {
            var assembly = typeof(Building).Assembly;
            var type = assembly.GetType(message.Type);

            return JsonConvert.DeserializeObject(message.CrabItem, type);
        }
    }
}