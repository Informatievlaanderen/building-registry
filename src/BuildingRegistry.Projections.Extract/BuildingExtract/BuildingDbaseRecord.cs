namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class BuildingDbaseRecord : DbaseRecord
    {
        public static readonly BuildingDbaseSchema Schema = new BuildingDbaseSchema();

        public DbaseCharacter id { get; }
        public DbaseInt32 gebouwid { get; }
        public DbaseCharacter versieid { get; }
        public DbaseCharacter geommet { get; }
        public DbaseCharacter status { get; }

        public BuildingDbaseRecord()
        {
            id = new DbaseCharacter(Schema.id);
            gebouwid = new DbaseInt32(Schema.gebouwid);
            versieid = new DbaseCharacter(Schema.versieid);
            geommet = new DbaseCharacter(Schema.geommet);
            status = new DbaseCharacter(Schema.status);

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
