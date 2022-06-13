namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentAssertions;
    using Xunit;

    public class OsloPuriValidatorTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("test/123", false)]
        [InlineData("http://test/123", true)]
        public void GivenInvalidPostInfoIdUri_Invalid(string puri, bool expectedResult)
        {
            OsloPuriValidator.TryParseIdentifier(puri, out _).Should().Be(expectedResult);
        }
    }
}
