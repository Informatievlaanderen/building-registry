namespace BuildingRegistry.Importer.Console.TestClient
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
    using Microsoft.Extensions.Configuration;

    internal class SettingsBasedConfig : IHttpApiProxyConfig, ICommandProcessorConfig, ICommandProcessorBatchConfiguration<int>
    {
        public SettingsBasedConfig(IConfiguration configuration)
        {
            BaseUrl = new Uri(configuration["BaseUrl"]);
            ImportEndpoint = configuration["ImportEndpoint"];
            ImportBatchStatusEndpoint = configuration["ImportBatchStatusEndpoint"];
        }

        public Uri BaseUrl { get; }
        public string ImportEndpoint { get; }
        public string ImportBatchStatusEndpoint { get; }
        public int HttpTimeoutMinutes => 5;
        public string AuthUserName => "";
        public string AuthPassword => "";
        public int NrOfProducers => 1;
        public int BufferSize => 1;
        public int NrOfConsumers => 1;
        public int BatchSize => 1;
        public bool WaitForUserInput => false;
        public TimeSpan TimeMargin => new TimeSpan(0, 0, 0);

        public override string ToString() => $"{Environment.NewLine}" +
                                             $"BaseUrl: {BaseUrl}{Environment.NewLine}" +
                                             $"ImportEndpoint: {ImportEndpoint}{Environment.NewLine}" +
                                             $"ImportBatchStatusEndpoint: {ImportBatchStatusEndpoint}{Environment.NewLine}" +
                                             $"HttpTimeoutMinutes: {HttpTimeoutMinutes}{Environment.NewLine}" +
                                             $"ImortAuthUserName: {AuthUserName}{Environment.NewLine}" +
                                             $"TimeMargin: {TimeMargin}{Environment.NewLine}" +
                                             $"NrOfProducers: {NrOfProducers}{Environment.NewLine}" +
                                             $"BufferSize: {BufferSize}{Environment.NewLine}" +
                                             $"NrOfConsumers: {NrOfConsumers}{Environment.NewLine}" +
                                             $"BatchSize: {BatchSize}";//{Environment.NewLine}";

        public int Deserialize(string key) => int.Parse(key);

    }
}
