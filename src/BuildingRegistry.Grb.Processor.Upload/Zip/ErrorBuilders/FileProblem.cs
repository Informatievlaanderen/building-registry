namespace BuildingRegistry.Grb.Processor.Upload.Zip.ErrorBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core;

    public abstract class FileProblem : IEquatable<FileProblem>, IEqualityComparer<FileProblem>
    {
        protected FileProblem(string file, string reason, IReadOnlyCollection<ProblemParameter> parameters)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string File { get; }
        public IReadOnlyCollection<ProblemParameter> Parameters { get; }
        public string Reason { get; }

        public bool Equals(FileProblem x, FileProblem y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null) return false;

            if (y is null) return false;

            if (x.GetType() != y.GetType()) return false;

            return x.File == y.File
                   && x.Reason == y.Reason
                   && Equals(x.Parameters, y.Parameters);
        }

        public virtual bool Equals(FileProblem other)
        {
            return other != null
                   && GetType() == other.GetType()
                   && string.Equals(File, other.File, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(Reason, other.Reason)
                   && Parameters.SequenceEqual(other.Parameters);
        }

        public int GetHashCode(FileProblem obj)
        {
            return HashCode.Combine(obj.File, obj.Reason, obj.Parameters);
        }

        public override bool Equals(object obj)
        {
            return obj is FileProblem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Parameters.Aggregate(
                File.GetHashCode() ^ Reason.GetHashCode(),
                (current, parameter) => current ^ parameter.GetHashCode());
        }

        public abstract Messages.FileProblem Translate();
    }
}
