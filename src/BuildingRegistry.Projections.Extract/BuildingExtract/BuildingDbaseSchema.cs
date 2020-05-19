namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class BuildingDbaseSchema : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField gebouwid => Fields[1];
        public DbaseField versieid => Fields[2];
        public DbaseField geommet => Fields[3];
        public DbaseField status => Fields[4];

        public BuildingDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(50)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(gebouwid)), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(versieid)), new DbaseFieldLength(25)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(geommet)), new DbaseFieldLength(20)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(20))
        };
    }
}
