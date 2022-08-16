namespace BuildingRegistry.Api.CrabImport.Abstractions.Post
{
    using System.ComponentModel.DataAnnotations;
    using System.Dynamic;
    using Legacy;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class RegisterCrabImportRequest
    {
        /// <summary>Type van het CRAB item.</summary>
        [Required]
        public string Type { get; }

        /// <summary>Het CRAB item.</summary>
        [Required]
        public string CrabItem { get; }

        public RegisterCrabImportRequest(string type, string crabItem)
        {
            Type = type;
            CrabItem = crabItem;
        }
    }

    public class RegisterCrabImportRequestExample : IExamplesProvider<RegisterCrabImportRequest>
    {
        public RegisterCrabImportRequest GetExamples() => new RegisterCrabImportRequest("BuildingRegistry.Municipality.Commands.ImportMunicipalityNameFromCrab", "{}");
    }

    public static class RegisterCrabImportRequestMapping
    {
        public static dynamic Map(RegisterCrabImportRequest message)
        {
            var assembly = typeof(Building).Assembly;
            var type = assembly.GetType(MakeNamespaceBackwardsCompatible(message))!;

            var result = JsonConvert.DeserializeObject(message.CrabItem, type);
            return result ?? new ExpandoObject();
        }

        private static string MakeNamespaceBackwardsCompatible(RegisterCrabImportRequest message)
            => message.Type.Replace("BuildingRegistry.Building.Commands.Crab", "BuildingRegistry.Legacy.Commands.Crab");
    }
}
