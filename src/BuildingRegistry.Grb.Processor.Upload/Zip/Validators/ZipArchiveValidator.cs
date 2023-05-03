namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using ErrorBuilders;
    using Exceptions;
    using Microsoft.Extensions.Logging;

    public class ZipArchiveValidator : IZipArchiveValidator
    {
        private static readonly string[] ValidationOrder = {
            "GEBOUW_ALL.DBF",
            "GEBOUW_ALL.SHP",
        };

        private readonly Dictionary<string, IZipArchiveEntryValidator> _validators;

        public ZipArchiveValidator(
            Encoding encoding,
            ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(encoding);

            _validators = new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "gebouw_ALL.dbf",
                    new ZipArchiveDbaseEntryValidator<GrbDbaseRecord>(
                        encoding,
                        new DbaseFileHeaderReadBehavior(true),
                        new GrbDbaseSchema(),
                        new GrbDbaseRecordsValidator())
                },
                {
                    "gebouw_ALL.shp",
                    new ZipArchiveShapeEntryValidator(encoding, new GrbShapeRecordsValidator())
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
            try
            {
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

                    foreach (var file in requiredFiles.OrderBy(file =>
                                 Array.IndexOf(ValidationOrder, file.ToUpperInvariant())))
                    {
                        if (_validators.TryGetValue(file, out var validator))
                        {
                            var fileProblems = validator.Validate(archive.GetEntry(file));
                            var fileProblemsGrouped = fileProblems
                                .SelectMany(kvp =>
                                    kvp.Value.Select(errorType =>
                                        new { ErrorType = errorType, RecordNumber = kvp.Key }))
                                .GroupBy(x => x.ErrorType)
                                .ToDictionary(group => group.Key, group => group.Select(x => x.RecordNumber).ToList());

                            foreach (var kvp in fileProblemsGrouped)
                            {
                                problems.Add(new FileError(file, kvp.Key.ToString(),
                                    new ProblemParameter("recordnumbers", string.Join(",", kvp.Value))));
                            }
                        }
                    }
                }
            }
            catch (ShapeHeaderFormatException ex)
            {
                problems += new FileError(ex.FileName, nameof(ShapeHeaderFormatException), new ProblemParameter("Exception", ex.InnerException!.ToString()));
            }
            catch (DbaseHeaderFormatException ex)
            {
                problems += new FileError(ex.FileName, nameof(ShapeHeaderFormatException), new ProblemParameter("Exception", ex.InnerException!.ToString()));
            }
            catch (DbaseHeaderSchemaMismatchException ex)
            {
                problems += new FileError(ex.FileName, nameof(ShapeHeaderFormatException));
            }

            return problems;
        }
    }
}
