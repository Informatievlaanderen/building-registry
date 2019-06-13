namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class BuildingUnitDbaseRecord : DbaseRecord
    {
        public static readonly BuildingUnitDbaseSchema Schema = new BuildingUnitDbaseSchema();

        public DbaseString id { get; }
        public DbaseInt32 gebouwehid { get; }
        public DbaseDateTime versieid { get; }
        public DbaseString gebouwid { get; }
        public DbaseString functie { get; }
        public DbaseString status { get; }
        public DbaseString posgeommet { get; }

        public BuildingUnitDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            gebouwehid = new DbaseInt32(Schema.gebouwehid);
            versieid = new DbaseDateTime(Schema.versieid);
            gebouwid = new DbaseString(Schema.gebouwid);
            functie = new DbaseString(Schema.functie);
            status = new DbaseString(Schema.status);
            posgeommet = new DbaseString(Schema.posgeommet);

            Values = new DbaseFieldValue[]
            {
                id,
                gebouwehid,
                versieid,
                gebouwid,
                functie,
                status,
                posgeommet
            };
        }
    }
}
