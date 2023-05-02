namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Grb.Processor.Upload.Zip;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
    using Microsoft.Extensions.Logging;
    using Translators;

    public class ZipArchiveValidator : IZipArchiveValidator
    {
        private static readonly string[] TranslationOrder = {
            "GEBOUW_ALL.DBF",
            "GEBOUW_ALL.SHP",
        };

        private readonly Dictionary<string, IZipArchiveEntryValidator> _translators;

        public ZipArchiveValidator(
            Encoding encoding,
            ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(encoding);

            _translators = new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                // {
                //     "gebouw_ALL.dbf",
                //     new ZipArchiveEntryValidator(
                //         encoding,
                //         new DbaseFileHeaderReadBehavior(true),
                //         new GrbDbaseRecordsTranslator())
                // },
                {
                    "gebouw_ALL.shp",
                    new ZipArchiveShapeEntryValidator(encoding, new GrbShapeRecordsTranslator())
                }
            };
        }

        public ZipArchiveProblems Validate(ZipArchive archive)
        {
            ArgumentNullException.ThrowIfNull(archive);

            var problems = ZipArchiveProblems.None;

            // Report all missing required files
            var missingRequiredFiles = new HashSet<string>(
                _validators.Keys,
                StringComparer.InvariantCultureIgnoreCase
            );
            missingRequiredFiles.ExceptWith(
                new HashSet<string>(
                    archive.Entries.Select(entry => entry.FullName),
                    StringComparer.InvariantCultureIgnoreCase
                )
            );
            problems = missingRequiredFiles.Aggregate(
                problems,
                (current, file) => current.RequiredFileMissing(file));

            // Validate all required files (if a validator was registered for it)

            if (missingRequiredFiles.Count == 0)
            {
                var requiredFiles = new HashSet<string>(
                    archive.Entries.Select(entry => entry.FullName),
                    StringComparer.InvariantCultureIgnoreCase
                );
                requiredFiles.IntersectWith(
                    new HashSet<string>(
                        _validators.Keys,
                        StringComparer.InvariantCultureIgnoreCase
                    )
                );

                var context = ZipArchiveValidationContext.Empty.WithZipArchiveMetadata(metadata);

                foreach (var file in
                         requiredFiles
                             .OrderBy(file => Array.IndexOf(ValidationOrder, file.ToUpperInvariant())))
                    if (_validators.TryGetValue(file, out var validator))
                    {
                        var (fileProblems, fileContext) = validator.Validate(archive.GetEntry(file), context);
                        problems = problems.AddRange(fileProblems);
                        context = fileContext;
                    }
            }

            return problems;
        }
    }
}
