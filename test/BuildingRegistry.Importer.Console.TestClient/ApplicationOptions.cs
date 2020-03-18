namespace BuildingRegistry.Importer.Console.TestClient
{
    public class ApplicationOptions
    {
        public string ImportEndPoint { get; set; }
        public int HttpTimeoutInMinutes { get; set; }
        public int NrOfProducers { get; set; }
        public int BufferSize { get; set; }
        public int NrOfConsumers { get; set; }
        public int BatchSize { get; set; }
        public int TimeMarginInMinutes { get; set; }
        public string BaseUrl { get; set; }
        public string ImportBatchStatusEndpoint { get; set; }
    }
}
