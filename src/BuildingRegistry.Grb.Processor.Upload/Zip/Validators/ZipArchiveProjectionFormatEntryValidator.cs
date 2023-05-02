// namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
//
// using System;
// using System.IO;
// using System.IO.Compression;
// using System.Text;
//
// public class ZipArchiveProjectionFormatEntryValidator : IZipArchiveEntryValidator
// {
//     private readonly Encoding _encoding;
//
//     public ZipArchiveProjectionFormatEntryValidator(Encoding encoding)
//     {
//         _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
//     }
//
//     public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
//     {
//         ArgumentNullException.ThrowIfNull(entry);
//         ArgumentNullException.ThrowIfNull(context);
//
//         var problems = ZipArchiveProblems.None;
//         using (var stream = entry.Open())
//         using (var reader = new StreamReader(stream, _encoding))
//         {
//             var projectionFormat = ProjectionFormat.Read(reader);
//             if (!projectionFormat.IsBelgeLambert1972()) problems += entry.ProjectionFormatInvalid();
//         }
//
//         return (problems, context);
//     }
// }
