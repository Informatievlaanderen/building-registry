// namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.IO.Compression;
// using System.Linq;
// using Be.Vlaanderen.Basisregisters.Shaperon;
//
// public class ZipArchiveVersionedDbaseEntryValidator : IZipArchiveEntryValidator
// {
//     private readonly IEnumerable<IZipArchiveDbaseEntryValidator> _versionedValidators;
//
//     public ZipArchiveVersionedDbaseEntryValidator(params IZipArchiveDbaseEntryValidator[] validators)
//     {
//         _versionedValidators = validators;
//     }
//
//     public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry,
//         ZipArchiveValidationContext context)
//     {
//         ArgumentNullException.ThrowIfNull(entry);
//         ArgumentNullException.ThrowIfNull(context);
//
//         var problems = ZipArchiveProblems.None;
//
//         DbaseSchema uploadedSchema = null;
//
//         foreach (var validator in _versionedValidators)
//         {
//             using (var stream = entry.Open())
//             using (var reader = new BinaryReader(stream, validator.Encoding))
//             {
//                 var header = DbaseFileHeader.Read(reader, validator.HeaderReadBehavior);
//                 uploadedSchema = header.Schema;
//
//                 if (!header.Schema.Equals(validator.Schema))
//                 {
//                     continue;
//                 }
//
//                 return validator.Validate(entry, context);
//             }
//         }
//
//         problems = problems.Add(entry.HasDbaseSchemaMismatch(_versionedValidators.First().Schema, uploadedSchema));
//
//         return (problems, context);
//     }
// }
