namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class BuildingUnitDbaseRecord : DbaseRecord
    {
        public static readonly BuildingUnitDbaseSchema Schema = new BuildingUnitDbaseSchema();

        public DbaseCharacter id { get; }
        public DbaseInt32 gebouwehid { get; }
        public DbaseCharacter versieid { get; }
        public DbaseCharacter gebouwid { get; }
        public DbaseCharacter functie { get; }
        public DbaseCharacter status { get; }
        public DbaseCharacter posgeommet { get; }

        public BuildingUnitDbaseRecord()
        {
            id = new DbaseCharacter(Schema.id);
            gebouwehid = new DbaseInt32(Schema.gebouwehid);
            versieid = new DbaseCharacter(Schema.versieid);
            gebouwid = new DbaseCharacter(Schema.gebouwid);
            functie = new DbaseCharacter(Schema.functie);
            status = new DbaseCharacter(Schema.status);
            posgeommet = new DbaseCharacter(Schema.posgeommet);

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
