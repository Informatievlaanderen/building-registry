using System;
using System.Globalization;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Newtonsoft.Json;
using NodaTime;

namespace BuildingRegistry.ValueObjects
{
    public class ReaddressingBeginDate : StructDataTypeValueObject<ReaddressingBeginDate, LocalDate>, IComparable
    {
        public ReaddressingBeginDate([JsonProperty("value")] LocalDate readdressingBeginDate) : base(readdressingBeginDate)  { }

        public override string ToString()
        {
            return this.Value.ToString("dd/MM/yyyy", (IFormatProvider)CultureInfo.InvariantCulture);
        }

        int IComparable.CompareTo(object obj)
        {
            if(obj is ReaddressingBeginDate date)
                return this.Value.CompareTo(date);
            else
            {
                return -1;
            }
        }
    }
}
