namespace BuildingRegistry.Grb.Abstractions
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class JobResultDbaseSchema : DbaseSchema
    {
        public DbaseField idn => Fields[0];
        public DbaseField grid => Fields[1];

        public JobResultDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(idn)), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(grid)), new DbaseFieldLength(10), new DbaseDecimalCount(0))
        };
    }
}
