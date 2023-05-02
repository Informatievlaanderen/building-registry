namespace BuildingRegistry.Grb.Processor.Upload.Zip.Core;

using System;
using System.Collections.Generic;

public class ProblemParameter : IEquatable<ProblemParameter>, IEqualityComparer<ProblemParameter>
{
    public ProblemParameter(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Name { get; }
    public string Value { get; }

    public bool Equals(ProblemParameter x, ProblemParameter y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null) return false;

        if (y is null) return false;

        if (x.GetType() != y.GetType()) return false;

        return x.Name == y.Name
               && x.Value == y.Value;
    }

    public virtual bool Equals(ProblemParameter other)
    {
        return other != null
               && string.Equals(Name, other.Name)
               && string.Equals(Value, other.Value);
    }

    public int GetHashCode(ProblemParameter obj)
    {
        return HashCode.Combine(obj.Name, obj.Value);
    }

    public override bool Equals(object obj)
    {
        return obj is ProblemParameter other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode() ^ Value.GetHashCode();
    }

    public Messages.ProblemParameter Translate()
    {
        return new Messages.ProblemParameter
        {
            Name = Name, Value = Value
        };
    }
}
