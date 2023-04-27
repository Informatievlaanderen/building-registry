namespace BuildingRegistry.Grb.Abstractions
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class JobResultDbaseRecord : DbaseRecord
    {
        public static readonly JobResultDbaseSchema Schema = new JobResultDbaseSchema();

        public DbaseInt32 idn { get; }
        public DbaseInt32 grid { get; }

        public JobResultDbaseRecord()
        {
            idn = new DbaseInt32(Schema.idn);
            grid = new DbaseInt32(Schema.grid);

            Values = new DbaseFieldValue[]
            {
                idn,
                grid
            };
        }
    }
}
