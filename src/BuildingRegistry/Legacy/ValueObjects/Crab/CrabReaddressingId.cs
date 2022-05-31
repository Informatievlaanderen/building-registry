namespace BuildingRegistry.Legacy.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabReaddressingId : IntegerValueObject<CrabReaddressingId>
    {
        public CrabReaddressingId([JsonProperty("value")] int readdressingId) : base(readdressingId) { }
    }
}
