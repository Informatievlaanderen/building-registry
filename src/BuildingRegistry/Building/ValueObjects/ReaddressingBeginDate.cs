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

        int IComparable.CompareTo(object obj) => obj is ReaddressingBeginDate date ? Value.CompareTo(date) : -1;
    }
}
