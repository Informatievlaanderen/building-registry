namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Shaperon;


    public class GrbDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator
    {
        public IDictionary<RecordNumber, JobRecord> Translate(IDbaseRecordEnumerator<GrbDbaseRecord> records)
        {
            var jobRecords = new Dictionary<RecordNumber, JobRecord>();

            var recordNumber = RecordNumber.Initial;

            while (records.MoveNext())
            {
                var record = records.Current;
                jobRecords.Add(recordNumber, new JobRecord
                {
                    Idn = record.IDN.Value,
                    IdnVersion = record.IDNV.Value,
                    VersionDate = new DateTimeOffset(record.GVDV.Value!.Value),
                    EndDate = record.GVDE.Value.HasValue ? new DateTimeOffset(record.GVDE.Value!.Value) : null,
                    EventType = (GrbEventType)record.EventType.Value,
                    GrbObject = (GrbObject)record.GRBOBJECT.Value,
                    GrId = record.GRID.Value == "-9" ? -9 : record.GRID.Value.AsIdentifier().Map(int.Parse),
                    GrbObjectType = (GrbObjectType)record.TPC.Value
                });

                recordNumber = recordNumber.Next();
            }

            return jobRecords;
        }
    }
}
