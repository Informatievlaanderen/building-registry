namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Logging;

    public class ZipArchiveTranslator : IZipArchiveTranslator
    {
        private static readonly string[] TranslationOrder = {
            "GEBOUW_ALL.DBF",
            "GEBOUW_ALL.SHP",
        };

        private readonly ILogger? _logger;
        private readonly Dictionary<string, IZipArchiveEntryTranslator> _translators;

        public ZipArchiveTranslator(
            Encoding encoding,
            ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(encoding);

            _logger = logger;
            _translators = new Dictionary<string, IZipArchiveEntryTranslator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "gebouw_ALL.dbf",
                    new ZipArchiveDbaseEntryTranslator(
                        encoding,
                        new DbaseFileHeaderReadBehavior(true),
                        new GrbDbaseRecordsTranslator())
                },
                {
                    "gebouw_ALL.shp",
                    new ZipArchiveShapeEntryTranslator(encoding, new GrbShapeRecordsTranslator())
                }
            };
        }

        public IEnumerable<JobRecord> Translate(ZipArchive archive)
        {
            ArgumentNullException.ThrowIfNull(archive);

            var entries = archive
                .Entries
                .Where(entry => Array.IndexOf(TranslationOrder, entry.FullName.ToUpperInvariant()) != -1)
                .OrderBy(entry => Array.IndexOf(TranslationOrder, entry.FullName.ToUpperInvariant()))
                .ToArray();

            _logger?.LogInformation("Translating {Count} entries", entries.Length);

            return entries
                .Aggregate(
                    new Dictionary<RecordNumber, JobRecord>() as IDictionary<RecordNumber, JobRecord>,
                    (previousResult, entry) =>
                    {
                        var sw = Stopwatch.StartNew();
                        _logger?.LogInformation("Translating entry {Entry}...", entry.FullName);

                        var result = _translators[entry.FullName].Translate(entry, previousResult);

                        _logger?.LogInformation("Translating entry {Entry} completed in {Elapsed}", entry.FullName, sw.Elapsed);

                        return result;
                    }).Values;
        }
    }
}
