namespace BuildingRegistry.Tests.AggregateTests
{
    using System;
    using Building;
    using FluentAssertions;
    using Xunit;

    public class BuildingStatusTests
    {
        [Fact]
        public void GivenNonParseableString_ThenThrowsNotImplementedException()
        {
            Action act = () => BuildingStatus.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }

    public class BuildingGeometryMethodTests
    {
        [Fact]
        public void GivenNonParseableString_ThenThrowsNotImplementedException()
        {
            Action act = () => BuildingGeometryMethod.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }

    public class BuildingUnitStatusTests
    {
        [Fact]
        public void GivenNonParseableString_ThenThrowsNotImplementedException()
        {
            Action act = () => BuildingUnitStatus.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }

    public class BuildingUnitFunctionTests
    {
        [Fact]
        public void GivenNonParseableString_ThenThrowsNotImplementedException()
        {
            Action act = () => BuildingUnitFunction.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }

    public class BuildingUnitPositionGeometryMethodTests
    {
        [Fact]
        public void GivenNonParseableString_ThenThrowsNotImplementedException()
        {
            Action act = () => BuildingUnitPositionGeometryMethod.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }
}
