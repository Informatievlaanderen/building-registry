namespace BuildingRegistry.Grb.Processor.Upload.Zip;

using System;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using ErrorBuilders;

public static class ZipArchiveEntryProblems
{
    // builder

    public static IDbaseFileRecordProblemBuilder AtDbaseRecord(this ZipArchiveEntry entry, RecordNumber number)
    {
        return new FileProblemBuilder(entry.Name).AtDbaseRecord(number);
    }

    public static IShapeFileRecordProblemBuilder AtShapeRecord(this ZipArchiveEntry entry, RecordNumber number)
    {
        return new FileProblemBuilder(entry.Name).AtShapeRecord(number);
    }

    public static IFileErrorBuilder Error(this ZipArchiveEntry entry, string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);

        return new FileProblemBuilder(entry.Name).Error(reason);
    }

    public static FileError HasDbaseHeaderFormatError(this ZipArchiveEntry entry, Exception exception)
    {
        return new FileProblemBuilder(entry.Name).HasDbaseHeaderFormatError(exception);
    }

    public static FileError HasDbaseSchemaMismatch(this ZipArchiveEntry entry, DbaseSchema expectedSchema, DbaseSchema actualSchema)
    {
        return new FileProblemBuilder(entry.Name).HasDbaseSchemaMismatch(expectedSchema, actualSchema);
    }

    // dbase

    public static FileProblem HasNoDbaseRecords(this ZipArchiveEntry entry, bool treatAsWarning)
    {
        return new FileProblemBuilder(entry.Name).HasNoDbaseRecords(treatAsWarning);
    }

    // shape

    public static FileError HasNoShapeRecords(this ZipArchiveEntry entry)
    {
        return new FileProblemBuilder(entry.Name).HasNoShapeRecords();
    }

    public static FileError HasShapeHeaderFormatError(this ZipArchiveEntry entry, Exception exception)
    {
        return new FileProblemBuilder(entry.Name).HasShapeRecordFormatError(exception);
    }

    public static FileProblem HasTooManyDbaseRecords(this ZipArchiveEntry entry, int expectedCount, int actualCount)
    {
        return new FileProblemBuilder(entry.Name).HasTooManyDbaseRecords(expectedCount, actualCount);
    }

    // projection format

    public static FileError ProjectionFormatInvalid(this ZipArchiveEntry entry)
    {
        return new FileProblemBuilder(entry.Name).ProjectionFormatInvalid();
    }

    public static FileProblem RoadSegmentsWithoutLaneAttributes(this ZipArchiveEntry entry, RoadSegmentId[] segments)
    {
        return new FileProblemBuilder(entry.Name).RoadSegmentsWithoutLaneAttributes(segments);
    }

    public static FileProblem RoadSegmentsWithoutSurfaceAttributes(this ZipArchiveEntry entry, RoadSegmentId[] segments)
    {
        return new FileProblemBuilder(entry.Name).RoadSegmentsWithoutSurfaceAttributes(segments);
    }

    public static FileProblem RoadSegmentsWithoutWidthAttributes(this ZipArchiveEntry entry, RoadSegmentId[] segments)
    {
        return new FileProblemBuilder(entry.Name).RoadSegmentsWithoutWidthAttributes(segments);
    }

    public static IFileWarningBuilder Warning(this ZipArchiveEntry entry, string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);

        return new FileProblemBuilder(entry.Name).Warning(reason);
    }

    private sealed class FileProblemBuilder : IFileProblemBuilder, IDbaseFileRecordProblemBuilder, IShapeFileRecordProblemBuilder
    {
        private readonly string _file;
        private readonly ImmutableList<ProblemParameter> _parameters;

        public FileProblemBuilder(string file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
            _parameters = ImmutableList<ProblemParameter>.Empty;
        }

        private FileProblemBuilder(string file, ImmutableList<ProblemParameter> parameters)
        {
            _file = file;
            _parameters = parameters;
        }

        public IDbaseFileRecordProblemBuilder AtDbaseRecord(RecordNumber number)
        {
            return new FileProblemBuilder(_file, _parameters.Add(new ProblemParameter("RecordNumber", number.ToString())));
        }

        public IShapeFileRecordProblemBuilder AtShapeRecord(RecordNumber number)
        {
            return new FileProblemBuilder(_file, _parameters.Add(new ProblemParameter("RecordNumber", number.ToString())));
        }

        public IFileErrorBuilder Error(string reason)
        {
            ArgumentNullException.ThrowIfNull(reason);

            return new FileErrorBuilder(_file, reason, _parameters);
        }

        public IFileWarningBuilder Warning(string reason)
        {
            ArgumentNullException.ThrowIfNull(reason);

            return new FileWarningBuilder(_file, reason, _parameters);
        }

        private sealed class FileErrorBuilder : IFileErrorBuilder
        {
            private readonly string _file;
            private readonly ImmutableList<ProblemParameter> _parameters;
            private readonly string _reason;

            public FileErrorBuilder(
                string file,
                string reason,
                ImmutableList<ProblemParameter> parameters)
            {
                _file = file;
                _reason = reason;
                _parameters = parameters;
            }

            public FileError Build()
            {
                return new FileError(_file.ToUpperInvariant(), _reason, _parameters.ToArray());
            }

            public IFileErrorBuilder WithParameter(ProblemParameter parameter)
            {
                ArgumentNullException.ThrowIfNull(parameter);

                return new FileErrorBuilder(_file, _reason, _parameters.Add(parameter));
            }

            public IFileErrorBuilder WithParameters(params ProblemParameter[] parameters)
            {
                ArgumentNullException.ThrowIfNull(parameters);

                return new FileErrorBuilder(_file, _reason, _parameters.AddRange(parameters));
            }
        }

        private sealed class FileWarningBuilder : IFileWarningBuilder
        {
            private readonly string _file;
            private readonly ImmutableList<ProblemParameter> _parameters;
            private readonly string _reason;

            public FileWarningBuilder(
                string file,
                string reason,
                ImmutableList<ProblemParameter> parameters)
            {
                _file = file;
                _reason = reason;
                _parameters = parameters;
            }

            public FileWarning Build()
            {
                return new FileWarning(_file.ToUpperInvariant(), _reason, _parameters.ToArray());
            }

            public IFileWarningBuilder WithParameter(ProblemParameter parameter)
            {
                ArgumentNullException.ThrowIfNull(parameter);

                return new FileWarningBuilder(_file, _reason, _parameters.Add(parameter));
            }

            public IFileWarningBuilder WithParameters(params ProblemParameter[] parameters)
            {
                ArgumentNullException.ThrowIfNull(parameters);

                return new FileWarningBuilder(_file, _reason, _parameters.AddRange(parameters));
            }
        }
    }
}
