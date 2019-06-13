using Be.Vlaanderen.Basisregisters.AggregateSource;
using Newtonsoft.Json;

namespace BuildingRegistry.ValueObjects.Crab
{
    public class CrabReaddressingId : IntegerValueObject<CrabReaddressingId>
    {
        public CrabReaddressingId([JsonProperty("value")] int readdressingId) : base(readdressingId) { }
    }
}
