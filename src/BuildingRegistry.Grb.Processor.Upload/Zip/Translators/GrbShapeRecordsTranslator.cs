namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System.Collections.Generic;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

    public sealed class GrbShapeRecordsTranslator : IZipArchiveShapeRecordsTranslator
    {
        public IDictionary<RecordNumber, JobRecord> Translate(IEnumerator<ShapeRecord> records, IDictionary<RecordNumber, JobRecord> jobRecords)
        {
            while (records.MoveNext())
            {
                var record = records.Current;
                if (record.Content is PolygonShapeContent content)
                {
                    var jobRecord = jobRecords[record.Header.RecordNumber];
                    jobRecord.Geometry = GeometryTranslator.ToGeometryPolygon(content.Shape);
                }
            }

            return jobRecords;
        }
    }
}
