namespace BuildingRegistry.Tests.SnapshotVerifier
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.SnapshotVerifier;
    using Building;
    using FluentAssertions;
    using KellermanSoftware.CompareNetObjects;
    using Xunit;

    public class CompareBuildingUnitsTests
    {
        private readonly BuildingPersistentLocalId _buildingPersistentLocalId;
        private readonly BuildingUnit _buildingUnit1;
        private readonly BuildingUnits _buildingUnits1;

        private readonly CompareLogic _compareLogic;

        public CompareBuildingUnitsTests()
        {
            _buildingPersistentLocalId = new BuildingPersistentLocalId(1);

            _buildingUnit1 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(1),
                BuildingUnitFunction.Unknown,
                BuildingUnitStatus.Realized,
                new List<AddressPersistentLocalId>
                {
                    new(1)
                },
                new BuildingUnitPosition(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()),
                    BuildingUnitPositionGeometryMethod.AppointedByAdministrator),
                false);

            _buildingUnits1 = new BuildingUnits { _buildingUnit1 };

            var config = DefaultComparisonConfig.Instance;
            config.MaxDifferences = 100;
            _compareLogic = new CompareLogic(config);
        }

        [Fact]
        public void AreEqual()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeTrue();
        }

        [Fact]
        public void DifferentBuildingPersistentLocalId()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                new BuildingPersistentLocalId(_buildingPersistentLocalId + 1),
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
            result.Differences.Should().ContainSingle();
            result.DifferencesString.Should().Contain("_buildingPersistentLocalId");
        }

        [Fact]
        public void DifferentBuildingUnitPersistentLocalId()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(_buildingUnit1.BuildingUnitPersistentLocalId + 1),
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
        }

        [Fact]
        public void DifferentBuildingUnitFunction()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function == BuildingUnitFunction.Unknown ? BuildingUnitFunction.Common : BuildingUnitFunction.Unknown,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
        }

        [Fact]
        public void DifferentBuildingUnitStatus()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function,
                _buildingUnit1.Status == BuildingUnitStatus.Planned ? BuildingUnitStatus.Realized : BuildingUnitStatus.Planned,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
            result.Differences.Should().ContainSingle();
            result.DifferencesString.Should().Contain(nameof(BuildingUnit.Status));
        }

        [Fact]
        public void DifferentBuildingUnitPosition()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                    ? new BuildingUnitPosition(_buildingUnit1.BuildingUnitPosition.Geometry, BuildingUnitPositionGeometryMethod.DerivedFromObject)
                    : new BuildingUnitPosition(_buildingUnit1.BuildingUnitPosition.Geometry, BuildingUnitPositionGeometryMethod.AppointedByAdministrator),
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
        }

        [Fact]
        public void DifferentBuildingUnitIsRemoved()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                !_buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
        }

        [Fact]
        public void DifferentBuildingUnitAddressPersistentLocalIds()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                _buildingUnit1.BuildingUnitPersistentLocalId,
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                new List<AddressPersistentLocalId>(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
        }

        [Fact]
        public void DifferentCount()
        {
            var buildingUnit2 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(_buildingUnit1.BuildingUnitPersistentLocalId + 1),
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);

            var buildingUnit3 = BuildingUnit.Migrate(
                _ => { },
                _buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(_buildingUnit1.BuildingUnitPersistentLocalId + 2),
                _buildingUnit1.Function,
                _buildingUnit1.Status,
                _buildingUnit1.AddressPersistentLocalIds.ToList(),
                _buildingUnit1.BuildingUnitPosition,
                _buildingUnit1.IsRemoved);
            var buildingUnits2 = new BuildingUnits { buildingUnit2, buildingUnit3 };

            var result = _compareLogic.Compare(_buildingUnits1, buildingUnits2);

            result.AreEqual.Should().BeFalse();
            result.DifferencesString.Should().Contain("[BuildingUnits.Count,BuildingUnits.Count]");
        }
    }
}
