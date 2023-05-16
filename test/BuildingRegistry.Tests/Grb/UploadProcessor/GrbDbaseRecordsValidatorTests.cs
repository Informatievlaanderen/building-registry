namespace BuildingRegistry.Tests.Grb.UploadProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Upload.Zip;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
    using FluentAssertions;
    using Xunit;

    public class GrbDbaseRecordsValidatorTests
    {
        [Fact]
        public void WhenRecordsNull_ThrowException()
        {
            var func = () => new GrbDbaseRecordsValidator().Validate("dummy", null);
            func.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenRecordsEmpty_ThrowException()
        {
            var func = () =>
                new GrbDbaseRecordsValidator().Validate("dummy", new List<GrbDbaseRecord>().GetEnumerator());
            func.Should().Throw<NoDbaseRecordsException>();
        }

        private GrbDbaseRecord GetCreateValidRecord(){
            var record = new GrbDbaseRecord();
            record.IDN.Value = 1;
            record.IDNV.Value = 1;
            record.GVDV.Value = DateTime.Now;
            record.GVDE.Value = DateTime.Now;
            record.EventType.Value = (int) GrbEventType.Unknown;
            record.GRBOBJECT.Value = (int) GrbObject.Unknown;
            record.GRID.Value = "https://data.vlaanderen.be/id/gebouw/11111111";
            record.TPC.Value = (int) GrbObjectType.MainBuilding;
            return record;
        }

        [Fact]
        public void WhenRecordEventTypeIsNull_ThenValidationError()
        {
            var recordWithEventTypeNull = GetCreateValidRecord();
            recordWithEventTypeNull.EventType.Reset();

            var validationResult =  new GrbDbaseRecordsValidator().Validate(
                "dummy",
                new List<GrbDbaseRecord>
                {
                    recordWithEventTypeNull
                }.GetEnumerator());

            var v = validationResult.FirstOrDefault();
            v.Should().NotBeNull();
            v.Value.Should().Contain(ValidationErrorType.UnknownEventType);
        }

        [Fact]
        public void WhenRecordEventTypeIsInvalid_ThenValidationError()
        {
            var recordWithEventTypeNull = GetCreateValidRecord();
            recordWithEventTypeNull.EventType.Value = 11;

            var validationResult =  new GrbDbaseRecordsValidator().Validate(
                "dummy",
                new List<GrbDbaseRecord>
                {
                    recordWithEventTypeNull
                }.GetEnumerator());

            var v = validationResult.FirstOrDefault();
            v.Should().NotBeNull();
            v.Value.Should().Contain(ValidationErrorType.UnknownEventType);
        }

        [Fact]
        public void WhenRecordGridIsInValid_ThenValidationError()
        {
            var recordWithEventTypeNull = GetCreateValidRecord();
            recordWithEventTypeNull.GRID.Value = "https://data.vlaanderen.be/id/gebouw/123abc";

            var validationResult =  new GrbDbaseRecordsValidator().Validate(
                "dummy",
                new List<GrbDbaseRecord>
                {
                    recordWithEventTypeNull
                }.GetEnumerator());

            var v = validationResult.FirstOrDefault();
            v.Should().NotBeNull();
            v.Value.Should().Contain(ValidationErrorType.InvalidGrId);
        }

        [Theory]
        [InlineData("-9")]
        [InlineData("https://data.vlaanderen.be/id/gebouw/123")]
        public void WhenRecordGridIsValid_ThenNoValidationError(string grid)
        {
            var recordWithEventTypeNull = GetCreateValidRecord();
            recordWithEventTypeNull.GRID.Value = grid;

            var validationResult =  new GrbDbaseRecordsValidator().Validate(
                "dummy",
                new List<GrbDbaseRecord>
                {
                    recordWithEventTypeNull
                }.GetEnumerator());

            validationResult.Should().BeNullOrEmpty();
        }
    }
}
