namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;

using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

public class GrbShapeRecordsValidator : IZipArchiveShapeRecordsValidator
{
    public IDictionary<RecordNumber, List<ValidationErrorType>> Validate(
        ZipArchiveEntry entry,
        IEnumerator<ShapeRecord> records)
    {
        var validationErrors = new Dictionary<RecordNumber, List<ValidationErrorType>>();

        var moved = records.MoveNext();

        if (!moved)
        {
            return validationErrors;
        }

        while (moved)
        {
            var record = records.Current;
            if (record.Content.ShapeType != ShapeType.Polygon)
            {
                validationErrors.Add(record.Header.RecordNumber, new List<ValidationErrorType>
                {
                    ValidationErrorType.GeometryIsNotPolygon
                });
            }
            else if (record.Content is not PolygonShapeContent content
                     || !GeometryValidator.IsValid(GeometryTranslator.ToGeometryPolygon(content.Shape)))
            {
                validationErrors.Add(record.Header.RecordNumber, new List<ValidationErrorType>
                {
                    ValidationErrorType.PolygonNotValid
                });
            }

            moved = records.MoveNext();
        }

        return validationErrors;
    }
}
