namespace BuildingRegistry.Importer.Console.TestClient
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;

    public class NonPersistentProcessedKeysSet<T> : IProcessedKeysSet<T>
    {
        private readonly List<T> _processedKeys = new List<T>();

        public bool Contains(T key) => _processedKeys.Contains(key);

        public void Add(IEnumerable<T> keys) => _processedKeys.AddRange(keys);

        public void Clear() => _processedKeys.Clear();
    }
}
