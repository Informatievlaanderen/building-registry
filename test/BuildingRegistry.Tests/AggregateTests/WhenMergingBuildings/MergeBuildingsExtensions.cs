// namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
// {
//     using Building.Commands;
//     using Building.Events;
//
//     public static class MergeBuildingsExtensions
//     {
//         public static BuildingMergerWasRealized ToBuildingMergerWasRealizedEvent(this MergeBuildings command)
//         {
//             return new BuildingMergerWasRealized(
//                 command.NewBuildingPersistentLocalId,
//                 command.NewExtendedWkbGeometry,
//                 command.BuildingPersistentLocalIdsToMerge);
//         }
//     }
// }
