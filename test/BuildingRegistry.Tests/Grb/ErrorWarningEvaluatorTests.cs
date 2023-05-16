namespace BuildingRegistry.Tests.Grb;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using BuildingRegistry.Grb.Abstractions;
using BuildingRegistry.Grb.Processor.Job;
using FluentAssertions;
using TicketingService.Abstractions;
using Xunit;

public class ErrorWarningEvaluatorTests
{
    [Fact]
    public void GivenOnlyErrorsInValidationErrors_ThenOutputError()
    {
        var validationErrors = new List<ValidationError>
        {
            new ValidationError("error1", "reason"),
            new ValidationError("error2", "reason2"),
        };

        var sut = new ErrorWarningEvaluator();

        // Act
        var (status, message) = sut.Evaluate(validationErrors);

        // Assert
        status.Should().Be(JobRecordStatus.Error);
        message.Should().Be($"reason{Environment.NewLine}reason2");
;    }

    [Theory]
    [InlineData("VerwijderdGebouw", "verwijderdeGebouw message")]
    public void GivenOnlyWarningsInValidationErrors_ThenOutputWarning(string code, string message)
    {
        var validationErrors = new List<ValidationError>
        {
            new ValidationError(code, message),
        };

        var sut = new ErrorWarningEvaluator();

        // Act
        var (status, resultMessage) = sut.Evaluate(validationErrors);

        // Assert
        status.Should().Be(JobRecordStatus.Warning);
        resultMessage.Should().Be(message);
    }

    [Fact]
    public void GivenBothWarningAndErrorsInValidationErrors_ThenOutputError()
    {
        var validationErrors = new List<ValidationError>
        {
            new ValidationError("error1", "reason"),
            new ValidationError(ErrorWarningEvaluator.Warnings.First(), "warning message"),
            new ValidationError("error2", "reason2"),
        };

        var sut = new ErrorWarningEvaluator();

        // Act
        var (status, message) = sut.Evaluate(validationErrors);

        // Assert
        status.Should().Be(JobRecordStatus.Error);
        message.Should().Be($"reason{Environment.NewLine}reason2");
    }

    [Fact]
    public void GivenErrorInTicketError_ThenOutputError()
    {
        var ticketError = new TicketError("reason", "error1");

        var sut = new ErrorWarningEvaluator();

        // Act
        var (status, message) = sut.Evaluate(ticketError);

        // Assert
        status.Should().Be(JobRecordStatus.Error);
        message.Should().Be("reason");
    }

    [Theory]
    [InlineData("VerwijderdGebouw", "verwijderdeGebouw message")]
    public void GivenWarningInTicketError_ThenOutputWarning(string code, string message)
    {
        var ticketError = new TicketError(message, code);

        var sut = new ErrorWarningEvaluator();

        // Act
        var (status, resultMessage) = sut.Evaluate(ticketError);

        // Assert
        status.Should().Be(JobRecordStatus.Warning);
        resultMessage.Should().Be(message);
    }
}
