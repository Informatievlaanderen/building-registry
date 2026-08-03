namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using FluentAssertions;

    /// <summary>
    /// Compares GML geometries on their srsName and on their coordinates within a tolerance, rather than as exact
    /// strings.
    /// <para>
    /// A Lambert 72 ↔ 2008 transform runs on doubles through trigonometric functions, and .NET does not guarantee
    /// those to return identical last bits across operating systems. The same transform therefore lands a polygon on
    /// coordinates that differ around the 11th decimal between Linux and Windows, and the GML writer emits all 11 of
    /// them. Asserting the exact string ties the expectation to whichever platform produced it — green locally on
    /// Windows, red on the Linux build agent. Points are unaffected: they are written with 2 decimals.
    /// </para>
    /// </summary>
    public static class GmlAssertions
    {
        /// <summary>
        /// A micrometre: orders of magnitude above the transform's platform noise, and far below any difference that
        /// would matter for a coordinate in metres.
        /// </summary>
        public const double CoordinateTolerance = 1e-6;

        /// <summary>
        /// <see cref="ShouldBeEquivalentGml"/> as a predicate, for use inside a Moq argument matcher.
        /// </summary>
        public static bool IsEquivalentGml(string? actual, string? expected, double tolerance = CoordinateTolerance)
        {
            if (actual is null || expected is null)
            {
                return actual == expected;
            }

            var actualGeometry = Parse(actual);
            var expectedGeometry = Parse(expected);

            return actualGeometry.Name == expectedGeometry.Name
                   && actualGeometry.SrsName == expectedGeometry.SrsName
                   && actualGeometry.Ordinates.Count == expectedGeometry.Ordinates.Count
                   && actualGeometry.Ordinates
                       .Zip(expectedGeometry.Ordinates)
                       .All(ordinates => Math.Abs(ordinates.First - ordinates.Second) <= tolerance);
        }

        public static void ShouldBeEquivalentGml(this string actual, string expected, double tolerance = CoordinateTolerance)
        {
            var actualGeometry = Parse(actual);
            var expectedGeometry = Parse(expected);

            actualGeometry.Name.Should().Be(expectedGeometry.Name);
            actualGeometry.SrsName.Should().Be(expectedGeometry.SrsName);
            actualGeometry.Ordinates.Should().HaveCount(expectedGeometry.Ordinates.Count);

            for (var i = 0; i < expectedGeometry.Ordinates.Count; i++)
            {
                actualGeometry.Ordinates[i].Should().BeApproximately(
                    expectedGeometry.Ordinates[i],
                    tolerance,
                    "ordinate {0} should match, but the geometry was {1} instead of {2}",
                    i,
                    actual,
                    expected);
            }
        }

        private static (string Name, string? SrsName, IReadOnlyList<double> Ordinates) Parse(string gml)
        {
            var root = XDocument.Parse(gml).Root!;

            var ordinates = root
                .DescendantsAndSelf()
                .Where(element => element.Name.LocalName is "pos" or "posList")
                .SelectMany(element => element.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                .Select(ordinate => double.Parse(ordinate, CultureInfo.InvariantCulture))
                .ToArray();

            return (root.Name.LocalName, root.Attribute("srsName")?.Value, ordinates);
        }
    }
}
