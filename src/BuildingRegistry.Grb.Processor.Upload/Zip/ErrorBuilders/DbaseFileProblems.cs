namespace BuildingRegistry.Grb.Processor.Upload.Zip.ErrorBuilders;

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;

public static class DbaseFileProblems
{
    private static readonly NumberFormatInfo Provider = new()
    {
        NumberDecimalSeparator = "."
    };

    private static string Describe(DbaseSchema schema)
    {
        var builder = new StringBuilder();
        var index = 0;
        foreach (var field in schema.Fields)
        {
            if (index > 0) builder.Append(", ");
            builder.Append(field.Name.ToString());
            builder.Append("[");
            builder.Append(field.FieldType.ToString());
            builder.Append("(");
            builder.Append(field.Length.ToString());
            builder.Append(",");
            builder.Append(field.DecimalCount.ToString());
            builder.Append(")");
            builder.Append("]");
            index++;
        }

        return builder.ToString();
    }
    //
    // public static FileError DownloadIdDiffersFromMetadata(this IDbaseFileRecordProblemBuilder builder, string value, string expectedValue)
    // {
    //     return builder
    //         .Error(nameof(DownloadIdDiffersFromMetadata))
    //         .WithParameter(new ProblemParameter("Actual", value))
    //         .WithParameter(new ProblemParameter("Expected", expectedValue))
    //         .Build();
    // }

    public static FileError DownloadIdInvalidFormat(this IDbaseFileRecordProblemBuilder builder, string value)
    {
        return builder
            .Error(nameof(DownloadIdInvalidFormat))
            .WithParameter(new ProblemParameter("Actual", value))
            .Build();
    }

    public static FileError EndRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(EndRoadNodeIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError FromPositionEqualToOrGreaterThanToPosition(this IDbaseFileRecordProblemBuilder builder,
        double from, double to)
    {
        return builder
            .Error(nameof(FromPositionEqualToOrGreaterThanToPosition))
            .WithParameter(new ProblemParameter("From", from.ToString(Provider)))
            .WithParameter(new ProblemParameter("To", to.ToString(Provider)))
            .Build();
    }

    public static FileError FromPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
    {
        return builder
            .Error(nameof(FromPositionOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
            .Build();
    }

    public static FileError GradeSeparatedJunctionTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(GradeSeparatedJunctionTypeMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", GradeSeparatedJunctionType.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError HasDbaseHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return builder
            .Error(nameof(HasDbaseHeaderFormatError))
            .WithParameter(new ProblemParameter("Exception", exception.ToString()))
            .Build();
    }

    // record

    public static FileError HasDbaseRecordFormatError(this IDbaseFileRecordProblemBuilder builder, Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return builder
            .Error(nameof(HasDbaseRecordFormatError))
            .WithParameter(new ProblemParameter("Exception", exception.ToString()))
            .Build();
    }

    public static FileError HasDbaseSchemaMismatch(this IFileProblemBuilder builder, DbaseSchema expectedSchema, DbaseSchema actualSchema)
    {
        if (expectedSchema == null) throw new ArgumentNullException(nameof(expectedSchema));
        if (actualSchema == null) throw new ArgumentNullException(nameof(actualSchema));

        return builder
            .Error(nameof(HasDbaseSchemaMismatch))
            .WithParameters(
                new ProblemParameter("ExpectedSchema", Describe(expectedSchema)),
                new ProblemParameter("ActualSchema", Describe(actualSchema))
            )
            .Build();
    }
    // file

    public static FileProblem HasNoDbaseRecords(this IFileProblemBuilder builder, bool treatAsWarning)
    {
        if (treatAsWarning)
            return builder.Warning(nameof(HasNoDbaseRecords)).Build();
        return builder.Error(nameof(HasNoDbaseRecords)).Build();
    }

    public static FileProblem HasTooManyDbaseRecords(this IFileProblemBuilder builder, int expectedCount, int actualCount)
    {
        return builder
            .Error(nameof(HasNoDbaseRecords))
            .WithParameters(
                new ProblemParameter("ExpectedCount", expectedCount.ToString()),
                new ProblemParameter("ActualCount", actualCount.ToString()))
            .Build();
    }

    public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
        AttributeId identifier,
        RecordNumber takenByRecordNumber)
    {
        return builder
            .Error(nameof(IdentifierNotUnique))
            .WithParameters(
                new ProblemParameter("Identifier", identifier.ToString()),
                new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
            .Build();
    }

    public static FileError IdentifierZero(this IDbaseFileRecordProblemBuilder builder)
    {
        return builder.Error(nameof(IdentifierZero)).Build();
    }

    // record type

    public static FileError RecordTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(RecordTypeMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RecordType.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RecordTypeNotSupported(this IDbaseFileRecordProblemBuilder builder, int actual, params int[] expected)
    {
        return builder
            .Error(nameof(RecordTypeNotSupported))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", expected.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RequiredFieldIsNull(this IDbaseFileRecordProblemBuilder builder, DbaseField field)
    {
        if (field == null) throw new ArgumentNullException(nameof(field));

        return builder
            .Error(nameof(RequiredFieldIsNull))
            .WithParameter(new ProblemParameter("Field", field.Name.ToString()))
            .Build();
    }
}
