using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BuildingRegistry.Importer.TestClient
{
    public class TestClientHttpApiProxy : IApiProxy
    {
        private readonly DateTime _fromDateTime;
        private readonly int _key;
        private readonly ImportMode _importMode;
        private readonly HttpApiProxy _httpApiProxy;

        public TestClientHttpApiProxy(
            ILogger logger,
            JsonSerializer serializer,
            IHttpApiProxyConfig config,
            DateTime fromDateTime,
            int key,
            ImportMode importMode)
        {
            _fromDateTime = fromDateTime;
            _key = key;
            _importMode = importMode;
            _httpApiProxy = new HttpApiProxy(logger, serializer, config, new ImportFeed());
        }

        public void ImportBatch<TKey>(IEnumerable<KeyImport<TKey>> imports)
        {
            _httpApiProxy.ImportBatch(imports);
        }

        public ICommandProcessorOptions<TKey> InitializeImport<TKey>(ImportOptions options,
            ICommandProcessorBatchConfiguration<TKey> configuration)
        {
            if (options != null)
                throw new ArgumentException($"{nameof(ImportOptions)} parameter is not supported for {nameof(TestClientHttpApiProxy)}, please pass null");

            return new CommandProcessorOptions<TKey>(
                    _fromDateTime,
                    DateTime.MaxValue,
                    new List<int> {_key}.Cast<TKey>(),
                    take: null,
                    cleanStart: true,
                    _importMode);
        }

        public void FinalizeImport<TKey>(ICommandProcessorOptions<TKey> options)
        { }
    }

    public class TestClientHttpApiProxyFactory : IApiProxyFactory
    {
        private readonly ILogger _logger;
        private readonly IHttpApiProxyConfig _config;
        private readonly DateTime _fromDateTime;
        private readonly int _key;
        private readonly ImportMode _importMode;

        public TestClientHttpApiProxyFactory(
            ILogger logger,
            IHttpApiProxyConfig config,
            DateTime fromDateTime,
            int key,
            ImportMode importMode)
        {
            _logger = logger;
            _config = config;
            _fromDateTime = fromDateTime;
            _key = key;
            _importMode = importMode;
        }

        public IApiProxy Create()
        {
            return new TestClientHttpApiProxy(
                _logger,
                JsonSerializer.CreateDefault(new JsonSerializerSettings().ConfigureForCrabImports()),
                _config,
                _fromDateTime,
                _key,
                _importMode);
        }
    }
}
