namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Exceptions;

    public class GrbDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<GrbDbaseRecord>
    {
        public IDictionary<RecordNumber, List<ValidationErrorType>> Validate(
            string zipArchiveEntryName,
            IEnumerator<GrbDbaseRecord> records)
        {
            ArgumentNullException.ThrowIfNull(records);

            var validationErrors = new Dictionary<RecordNumber, List<ValidationErrorType>>();

            var moved = records.MoveNext();
            if (!moved)
            {
                throw new NoDbaseRecordsException(zipArchiveEntryName);
            }

            var index = 1;
            while (moved)
            {
                var record = records.Current;

                if (!record.EventType.HasValue ||
                    (record.EventType.HasValue && !Enum.IsDefined(typeof(GrbEventType), record.EventType.Value)))
                {
                    validationErrors.Add(new RecordNumber(index), new List<ValidationErrorType>
                    {
                        ValidationErrorType.UnknownEventType
                    });
                }

                if (record.GRID.Value != "-9" && !int.TryParse(record.GRID.Value.AsIdentifier().Value, out _))
                {
                    validationErrors.Add(new RecordNumber(index), new List<ValidationErrorType>
                    {
                        ValidationErrorType.InvalidGrId
                    });
                }

                moved = records.MoveNext();

                ++index;
            }

            return validationErrors;
        }
    }
}
