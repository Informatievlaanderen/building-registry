namespace BuildingRegistry.Legacy.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class CrabCoordinate : DecimalValueObject<CrabCoordinate>
    {
        public CrabCoordinate([JsonProperty("value")] decimal coordinate) : base(coordinate) { }
    }
}
