// namespace BuildingRegistry.Grb.Processor.Upload.Zip;
//
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Collections.Immutable;
// using System.Linq;
//
// public sealed class ZipArchiveValidationResult : IReadOnlyCollection<FileProblem>, IEquatable<ZipArchiveValidationResult>
// {
//     public static readonly ZipArchiveValidationResult Ok = new(
//         ZipArchiveProblems.None,
//         ImmutableHashSet<RoadSegmentId>.Empty);
//
//     private readonly ZipArchiveProblems _problems;
//     private readonly ImmutableHashSet<RoadSegmentId> _segments;
//
//     private ZipArchiveValidationResult(ZipArchiveProblems problems, ImmutableHashSet<RoadSegmentId> segments)
//     {
//         _problems = problems;
//         _segments = segments;
//     }
//
//     public int Count => _problems.Count;
//     public IReadOnlyCollection<RoadSegmentId> RoadSegments => _segments;
//
//     public bool Equals(ZipArchiveValidationResult other)
//     {
//         return other != null
//                && _problems.SequenceEqual(other._problems)
//                && _segments.SetEquals(other._segments);
//     }
//
//     public IEnumerator<FileProblem> GetEnumerator()
//     {
//         return _problems.GetEnumerator();
//     }
//
//     IEnumerator IEnumerable.GetEnumerator()
//     {
//         return GetEnumerator();
//     }
//
//     public ZipArchiveValidationResult Add(FileProblem problem)
//     {
//         if (problem == null) throw new ArgumentNullException(nameof(problem));
//
//         return new ZipArchiveValidationResult(_problems.Add(problem), _segments);
//     }
//
//     public ZipArchiveValidationResult AddRange(IEnumerable<FileProblem> problems)
//     {
//         if (problems == null) throw new ArgumentNullException(nameof(problems));
//
//         return new ZipArchiveValidationResult(_problems.AddRange(problems), _segments);
//     }
//
//     public ZipArchiveValidationResult AddRoadSegment(RoadSegmentId id)
//     {
//         return new ZipArchiveValidationResult(_problems, _segments.Add(id));
//     }
//
//     public bool ContainsRoadSegment(RoadSegmentId id)
//     {
//         return _segments.Contains(id);
//     }
//
//     public override bool Equals(object obj)
//     {
//         return obj is ZipArchiveValidationResult other && Equals(other);
//     }
//
//     public override int GetHashCode()
//     {
//         return _problems.Aggregate(0, (current, error) => current ^ error.GetHashCode())
//                ^
//                _segments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode());
//     }
//
//     public static ZipArchiveValidationResult operator +(ZipArchiveValidationResult left, FileProblem right)
//     {
//         return left.Add(right);
//     }
//
//     public static ZipArchiveValidationResult operator +(ZipArchiveValidationResult left, IEnumerable<FileProblem> right)
//     {
//         return left.AddRange(right);
//     }
//
//     public static ZipArchiveValidationResult operator +(ZipArchiveValidationResult left, ZipArchiveProblems right)
//     {
//         return left.AddRange(right);
//     }
//
//     public ZipArchiveValidationResult RequiredFileMissing(string file)
//     {
//         if (file == null) throw new ArgumentNullException(nameof(file));
//
//         return new ZipArchiveValidationResult(
//             _problems.Add(new FileError(file.ToUpperInvariant(), nameof(RequiredFileMissing))),
//             _segments
//         );
//     }
// }
