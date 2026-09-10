namespace BuildingRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Building;
    using FluentAssertions;
    using Xunit;

    public class ExtendedWkbGeometryTests
    {
        private const string Wkt = "POINT (141298.83 185196.04)";

        /// <summary>
        /// <see cref="ExtendedWkbGeometry.ToByteArray"/> is the one public door onto the backing array —
        /// <c>Value</c> itself is protected. The equality components of a <c>ByteArrayValueObject</c> are its
        /// individual bytes, so handing the array out unguarded would let a caller change what an
        /// already-constructed geometry equals and hashes to, from the other side of the register.
        /// </summary>
        [Fact]
        public void WhenTheReturnedArrayIsMutated_ThenTheGeometryIsUnchanged()
        {
            var geometry = GeometryHelper.CreateEwkbFromWkt(Wkt, SystemReferenceId.SridLambert72);
            var unchanged = GeometryHelper.CreateEwkbFromWkt(Wkt, SystemReferenceId.SridLambert72);

            var bytes = geometry.ToByteArray()!;
            bytes[0] ^= 0xFF;
            bytes[^1] ^= 0xFF;

            geometry.ToByteArray().Should().Equal(unchanged.ToByteArray());
            geometry.Should().Be(unchanged);
            geometry.GetHashCode().Should().Be(unchanged.GetHashCode());
            geometry.ToString().Should().Be(unchanged.ToString());
        }

        [Fact]
        public void ThenEachCallReturnsADistinctArray()
        {
            var geometry = GeometryHelper.CreateEwkbFromWkt(Wkt, SystemReferenceId.SridLambert72);

            geometry.ToByteArray().Should().NotBeSameAs(geometry.ToByteArray());
            geometry.ToByteArray().Should().Equal(geometry.ToByteArray());
        }
    }
}
