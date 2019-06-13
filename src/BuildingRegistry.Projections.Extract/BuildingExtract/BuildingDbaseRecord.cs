namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class BuildingDbaseRecord : DbaseRecord
    {
        public static readonly BuildingDbaseSchema Schema = new BuildingDbaseSchema();

        public DbaseString id { get; }
        public DbaseInt32 gebouwid { get; }
        public DbaseDateTime versieid { get; }
        public DbaseString geommet { get; }
        public DbaseString status { get; }

        public BuildingDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            gebouwid = new DbaseInt32(Schema.gebouwid);
            versieid = new DbaseDateTime(Schema.versieid);
            geommet = new DbaseString(Schema.geommet);
            status = new DbaseString(Schema.status);

            Values = new DbaseFieldValue[]
            {
                id,
                gebouwid,
                versieid,
                geommet,
                status
            };
        }
    }
}
