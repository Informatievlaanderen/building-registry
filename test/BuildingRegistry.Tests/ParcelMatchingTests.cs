namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Building;
    using FluentAssertions;
    using Moq;
    using NetTopologySuite.Geometries;
    using Xunit;

    public sealed class ParcelMatchingTests
    {
        private readonly Fixture _fixture;

        public ParcelMatchingTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task WhenOverlapThenReturnsResult()
        {
            var buildingPolygonWkt =
                "POLYGON ((30359.924344554543 197007.54170677811, 30359.446008555591 197010.21338678151, 30371.943992562592 197013.23297078162, 30373.701176568866 197006.42113077641, 30363.939512558281 197004.00340277702, 30364.205112561584 197002.85997877643, 30357.719608552754 197001.36161077395, 30356.638264551759 197006.90023477748, 30359.924344554543 197007.54170677811, 30360.468344554305 197004.48564277589, 30362.562808558345 197004.85844277591, 30362.018680557609 197007.91457077861, 30359.924344554543 197007.54170677811))";

            var parcelPolygonWkt =
                "POLYGON ((30357.7194805518 197001.361546773, 30356.6382645518 197006.900234777, 30359.9243445545 197007.541706778, 30359.4460085556 197010.213386782, 30371.9439925626 197013.232970782, 30373.7009845674 197006.421002779, 30363.9393205568 197004.003274776, 30364.2048565596 197002.859850775, 30357.7194805518 197001.361546773))";

            var buildingPolygon = GeometryHelper.CreateGeometryFromWkt(buildingPolygonWkt);

            var parcelPolygon = GeometryHelper.CreateGeometryFromWkt(parcelPolygonWkt);
            var parcelPolygonNeighbour1 = GeometryHelper.CreateGeometryFromWkt(
                "POLYGON ((30352.9087925479 196994.308490768, 30347.8442165479 196993.40410677, 30346.2433845475 196998.867338773, 30357.7194805518 197001.361546773, 30364.2048565596 197002.859850775, 30363.9393205568 197004.003274776, 30373.7009845674 197006.421002779, 30373.744120568 197006.236746777, 30375.473080568 196998.856010772, 30375.4976565689 196998.751114771, 30375.5030325651 196998.728010774, 30363.8836405575 196996.425418772, 30355.8676405549 196994.836874768, 30352.9087925479 196994.308490768))");
            var parcelPolygonNeighbour2 = GeometryHelper.CreateGeometryFromWkt(
                "POLYGON ((30369.9705525637 197020.970058788, 30371.0035125613 197016.919882786, 30371.9439925626 197013.232970782, 30359.4460085556 197010.213386782, 30359.9243445545 197007.541706778, 30356.6382645518 197006.900234777, 30357.7194805518 197001.361546773, 30346.2433845475 196998.867338773, 30347.8442165479 196993.40410677, 30334.0098485351 196989.266250767, 30331.0075445399 196999.638474774, 30326.6319285333 197014.755978782, 30332.6787765399 197016.530698784, 30336.4549685419 197017.638986785, 30337.4389685392 197014.234186783, 30348.4625845477 197017.037578784, 30356.1769525558 197018.999370787, 30356.4889525548 197017.560394786, 30369.9705525637 197020.970058788))");

            var mock = new Mock<IParcels>();
            mock.Setup(x => x.GetUnderlyingParcelsUnderBoundingBox(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        new Guid("AF915A78-D740-532E-829C-3D7AC4CD0DC0"),
                        "38025A0489-00C000",
                        parcelPolygon,
                        "Realized",
                        new List<AddressPersistentLocalId> { _fixture.Create<AddressPersistentLocalId>() }
                    ),
                    new ParcelData(
                        new Guid("2CA9B7F3-8B13-5C00-97EB-C64E2CB47693"),
                        "38025A0489-00A000",
                        parcelPolygonNeighbour1,
                        "Realized",
                        new List<AddressPersistentLocalId> { _fixture.Create<AddressPersistentLocalId>() }
                    ),
                    new ParcelData(
                        new Guid("7183606E-2F80-5C89-8C94-EB3EA45ED377"),
                        "38025A0489-00F000",
                        parcelPolygonNeighbour2,
                        "Realized",
                        new List<AddressPersistentLocalId> { _fixture.Create<AddressPersistentLocalId>() }
                    )
                });

            var parcelMatching = new ParcelMatching(mock.Object);
            var result = await parcelMatching.GetUnderlyingParcels(buildingPolygon);

            result.Count().Should().Be(1);
        }
    }
}
