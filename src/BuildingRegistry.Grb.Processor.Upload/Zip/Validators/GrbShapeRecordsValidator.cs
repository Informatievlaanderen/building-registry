namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Exceptions;

    public class GrbShapeRecordsValidator : IZipArchiveShapeRecordsValidator
    {
        public IDictionary<RecordNumber, List<ValidationErrorType>> Validate(
            string zipArchiveEntryName,
            IEnumerator<ShapeRecord> records)
        {
            var validationErrors = new Dictionary<RecordNumber, List<ValidationErrorType>>();

            var moved = records.MoveNext();

            if (!moved)
            {
                throw new NoShapeRecordsException(zipArchiveEntryName);
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
                else if (!GeometryValidator.IsValid(GeometryTranslator.ToGeometryPolygon((record.Content as PolygonShapeContent)!.Shape)))
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
}
