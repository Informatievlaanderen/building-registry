namespace BuildingRegistry.Building
{
    using System;
    using System.Globalization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using NodaTime;

    public class ReaddressingBeginDate : StructDataTypeValueObject<ReaddressingBeginDate, LocalDate>, IComparable
    {
        public ReaddressingBeginDate(LocalDate readdressingBeginDate) : base(readdressingBeginDate)  { }

        public override string ToString() => Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        int IComparable.CompareTo(object? obj) => obj is ReaddressingBeginDate date ? Value.CompareTo(date) : -1;

        public override bool Equals(object obj) => Value.Equals(obj);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(ReaddressingBeginDate obj1, ReaddressingBeginDate obj2) => obj1.Equals(obj2);
        public static bool operator !=(ReaddressingBeginDate obj1, ReaddressingBeginDate obj2) => !obj1.Equals(obj2);
        public static bool operator <(ReaddressingBeginDate obj1, ReaddressingBeginDate obj2) => ((IComparable)obj1).CompareTo(obj2) < 0;
        public static bool operator <=(ReaddressingBeginDate obj1, ReaddressingBeginDate obj2) => ((IComparable)obj1).CompareTo(obj2) <= 0;
        public static bool operator >(ReaddressingBeginDate obj1, ReaddressingBeginDate obj2) => ((IComparable)obj1).CompareTo(obj2) > 0;
        public static bool operator >=(ReaddressingBeginDate obj1, ReaddressingBeginDate obj2) => ((IComparable)obj1).CompareTo(obj2) >= 0;
    }
}
