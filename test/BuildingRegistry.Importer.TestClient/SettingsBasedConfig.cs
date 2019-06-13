using System;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
using BuildingRegistry.Importer.TestClient.Properties;

namespace BuildingRegistry.Importer.TestClient
{
    internal class SettingsBasedConfig : IHttpApiProxyConfig, ICommandProcessorConfig, ICommandProcessorBatchConfiguration<int>
    {
        public Uri BaseUrl => new Uri(Settings.Default.BaseUrl);
        public string ImportEndpoint => Settings.Default.ImportEndpoint;
        public string ImportBatchStatusEndpoint => Settings.Default.ImportBatchStatusEndpoint;
        public int HttpTimeoutMinutes => 5;
        public string AuthUserName => "";
        public string AuthPassword => "";
        public int NrOfProducers => 1;
        public int BufferSize => 1;
        public int NrOfConsumers => 1;
        public int BatchSize => 1;
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
