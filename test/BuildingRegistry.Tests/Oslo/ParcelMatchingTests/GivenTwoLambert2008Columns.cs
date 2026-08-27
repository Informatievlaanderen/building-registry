namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    /// <summary>
    /// The parcel consumer and the building projection each have their own Lambert 2008 column, filling on
    /// independent schedules, and one <see cref="Lambert2008MatchingReadiness"/> singleton guards both. A
    /// guard that remembered only "verified" rather than "verified what" would let the first probe to pass
    /// wave the second through — the same silent pass it exists to prevent. See ADR 0006.
    /// </summary>
    public class GivenTwoLambert2008Columns
    {
        [Fact]
        public void WhenOneColumnIsVerified_ThenTheOtherIsStillProbed()
        {
            var readiness = new Lambert2008MatchingReadiness();

            readiness.EnsureVerified(Lambert2008MatchingReadiness.Parcels, () => false);

            var buildingsProbed = false;

            readiness.EnsureVerified(Lambert2008MatchingReadiness.Buildings, () =>
            {
                buildingsProbed = true;

                return false;
            });

            buildingsProbed.Should().BeTrue("a passing parcel probe says nothing about the building column");
        }

        [Fact]
        public void WhenOneColumnIsIncomplete_ThenTheOtherPassingDoesNotExcuseIt()
        {
            var readiness = new Lambert2008MatchingReadiness();

            readiness.EnsureVerified(Lambert2008MatchingReadiness.Parcels, () => false);

            var act = () => readiness.EnsureVerified(Lambert2008MatchingReadiness.Buildings, () => true);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*buildings*no Lambert 2008 geometry*");
        }

        [Fact]
        public void WhenAColumnHasPassed_ThenItIsNotProbedAgain()
        {
            var readiness = new Lambert2008MatchingReadiness();
            var probes = 0;

            for (var i = 0; i < 3; i++)
            {
                readiness.EnsureVerified(Lambert2008MatchingReadiness.Buildings, () =>
                {
                    probes++;

                    return false;
                });
            }

            probes.Should().Be(1, "the probe is a scan whose expensive case is the normal one");
        }
    }
}
