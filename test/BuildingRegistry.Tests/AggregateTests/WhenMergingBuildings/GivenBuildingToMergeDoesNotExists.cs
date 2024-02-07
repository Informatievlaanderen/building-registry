// namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
// {
//     using System.Linq;
//     using AutoFixture;
//     using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
//     using Building.Commands;
//     using Building.Exceptions;
//     using Xunit;
//     using Xunit.Abstractions;
//
//     public sealed class GivenBuildingToMergeDoesNotExists : BuildingRegistryTest
//     {
//         public GivenBuildingToMergeDoesNotExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
//         {
//         }
//
//         [Fact]
//         public void BuildingToMergeNotFoundExceptionWasThrown()
//         {
//             var command = Fixture.Create<MergeBuildings>();
//
//             Assert(new Scenario()
//                 .GivenNone()
//                 .When(command)
//                 .Throws(new BuildingToMergeNotFoundException(command.BuildingPersistentLocalIdsToMerge.First())));
//         }
//     }
// }
