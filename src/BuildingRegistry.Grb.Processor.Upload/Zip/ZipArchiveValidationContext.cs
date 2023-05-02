// namespace BuildingRegistry.Grb.Processor.Upload.Zip;
//
// using System;
// using System.Collections.Immutable;
// using System.Linq;
//
// public sealed class ZipArchiveValidationContext : IEquatable<ZipArchiveValidationContext>
// {
//     public static readonly ZipArchiveValidationContext Empty = new(
//         ImmutableHashSet<RoadSegmentId>.Empty,
//         ImmutableHashSet<RoadSegmentId>.Empty,
//         ImmutableHashSet<RoadSegmentId>.Empty,
//         ImmutableHashSet<RoadSegmentId>.Empty,
//         ImmutableHashSet<RoadNodeId>.Empty,
//         ImmutableHashSet<RoadNodeId>.Empty,
//         ImmutableHashSet<RoadNodeId>.Empty,
//         ImmutableHashSet<RoadNodeId>.Empty,
//         ZipArchiveMetadata.Empty);
//
//     private readonly ImmutableHashSet<RoadNodeId> _addedNodes;
//     private readonly ImmutableHashSet<RoadSegmentId> _addedSegments;
//     private readonly ImmutableHashSet<RoadNodeId> _identicalNodes;
//     private readonly ImmutableHashSet<RoadSegmentId> _identicalSegments;
//     private readonly ImmutableHashSet<RoadNodeId> _modifiedNodes;
//     private readonly ImmutableHashSet<RoadSegmentId> _modifiedSegments;
//     private readonly ImmutableHashSet<RoadNodeId> _removedNodes;
//     private readonly ImmutableHashSet<RoadSegmentId> _removedSegments;
//
//     private ZipArchiveValidationContext(ImmutableHashSet<RoadSegmentId> identicalSegments,
//         ImmutableHashSet<RoadSegmentId> addedSegments,
//         ImmutableHashSet<RoadSegmentId> modifiedSegments,
//         ImmutableHashSet<RoadSegmentId> removedSegments,
//         ImmutableHashSet<RoadNodeId> identicalNodes,
//         ImmutableHashSet<RoadNodeId> addedNodes,
//         ImmutableHashSet<RoadNodeId> modifiedNodes,
//         ImmutableHashSet<RoadNodeId> removedNodes,
//         ZipArchiveMetadata zipArchiveMetadata)
//     {
//         _identicalSegments = identicalSegments;
//         _addedSegments = addedSegments;
//         _modifiedSegments = modifiedSegments;
//         _removedSegments = removedSegments;
//         _identicalNodes = identicalNodes;
//         _addedNodes = addedNodes;
//         _modifiedNodes = modifiedNodes;
//         _removedNodes = removedNodes;
//         ZipArchiveMetadata = zipArchiveMetadata;
//     }
//
//     public IImmutableSet<RoadNodeId> KnownAddedRoadNodes => _addedNodes;
//     public IImmutableSet<RoadSegmentId> KnownAddedRoadSegments => _addedSegments;
//     public IImmutableSet<RoadNodeId> KnownIdenticalRoadNodes => _identicalNodes;
//     public IImmutableSet<RoadSegmentId> KnownIdenticalRoadSegments => _identicalSegments;
//     public IImmutableSet<RoadNodeId> KnownModifiedRoadNodes => _modifiedNodes;
//     public IImmutableSet<RoadSegmentId> KnownModifiedRoadSegments => _modifiedSegments;
//     public IImmutableSet<RoadNodeId> KnownRemovedRoadNodes => _removedNodes;
//     public IImmutableSet<RoadSegmentId> KnownRemovedRoadSegments => _removedSegments;
//
//     public IImmutableSet<RoadNodeId> KnownRoadNodes => _identicalNodes
//         .Union(_addedNodes)
//         .Union(_modifiedNodes)
//         .Union(_removedNodes);
//
//     public IImmutableSet<RoadSegmentId> KnownRoadSegments => _identicalSegments
//         .Union(_addedSegments)
//         .Union(_modifiedSegments)
//         .Union(_removedSegments);
//
//     public ZipArchiveMetadata ZipArchiveMetadata { get; }
//
//     public bool Equals(ZipArchiveValidationContext other)
//     {
//         return other != null
//                && _identicalSegments.SetEquals(other._identicalSegments)
//                && _addedSegments.SetEquals(other._addedSegments)
//                && _modifiedSegments.SetEquals(other._modifiedSegments)
//                && _removedSegments.SetEquals(other._removedSegments)
//                && _identicalNodes.SetEquals(other._identicalNodes)
//                && _addedNodes.SetEquals(other._addedNodes)
//                && _modifiedNodes.SetEquals(other._modifiedNodes)
//                && _removedNodes.SetEquals(other._removedNodes)
//                && ZipArchiveMetadata.Equals(other.ZipArchiveMetadata);
//     }
//
//     public override bool Equals(object obj)
//     {
//         return obj is ZipArchiveValidationContext other && Equals(other);
//     }
//
//     public override int GetHashCode()
//     {
//         return _identicalSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
//                ^ _addedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
//                ^ _modifiedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
//                ^ _removedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
//                ^ _identicalNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode())
//                ^ _addedNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode())
//                ^ _modifiedNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode())
//                ^ _removedNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode()
//                                                                        ^ ZipArchiveMetadata.GetHashCode());
//     }
//
//     public ZipArchiveValidationContext WithAddedRoadNode(RoadNodeId node)
//     {
//         return new ZipArchiveValidationContext(
//             _identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes.Add(node),
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithAddedRoadSegment(RoadSegmentId segment)
//     {
//         return new ZipArchiveValidationContext(
//             _identicalSegments,
//             _addedSegments.Add(segment),
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithIdenticalRoadNode(RoadNodeId node)
//     {
//         return new ZipArchiveValidationContext(
//             _identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes.Add(node),
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithIdenticalRoadSegment(RoadSegmentId segment)
//     {
//         return new ZipArchiveValidationContext(
//             _identicalSegments.Add(segment),
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithModifiedRoadNode(RoadNodeId node)
//     {
//         return new ZipArchiveValidationContext(_identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes.Add(node),
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithModifiedRoadSegment(RoadSegmentId segment)
//     {
//         return new ZipArchiveValidationContext(_identicalSegments,
//             _addedSegments,
//             _modifiedSegments.Add(segment),
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithRemovedRoadNode(RoadNodeId node)
//     {
//         return new ZipArchiveValidationContext(
//             _identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes.Add(node),
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithRemovedRoadSegment(RoadSegmentId segment)
//     {
//         return new ZipArchiveValidationContext(
//             _identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments.Add(segment),
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithRoadNode(RoadNodeId node, RecordType recordType)
//     {
//         if (recordType == RecordType.Added) return WithAddedRoadNode(node);
//
//         if (recordType == RecordType.Modified) return WithModifiedRoadNode(node);
//
//         if (recordType == RecordType.Removed) return WithRemovedRoadNode(node);
//
//         if (recordType == RecordType.Identical) return WithIdenticalRoadNode(node);
//
//         return new ZipArchiveValidationContext(_identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             ZipArchiveMetadata);
//     }
//
//     public ZipArchiveValidationContext WithZipArchiveMetadata(ZipArchiveMetadata zipArchiveMetadata)
//     {
//         return new ZipArchiveValidationContext(_identicalSegments,
//             _addedSegments,
//             _modifiedSegments,
//             _removedSegments,
//             _identicalNodes,
//             _addedNodes,
//             _modifiedNodes,
//             _removedNodes,
//             zipArchiveMetadata);
//     }
// }
