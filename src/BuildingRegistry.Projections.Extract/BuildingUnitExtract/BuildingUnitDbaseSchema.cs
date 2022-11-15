namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class BuildingUnitDbaseSchema : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField gebouwehid => Fields[1];
        public DbaseField versieid => Fields[2];
        public DbaseField gebouwid => Fields[3];
        public DbaseField functie => Fields[4];
        public DbaseField status => Fields[5];
        public DbaseField posgeommet => Fields[6];
        public DbaseField afwijking => Fields[7];

        public BuildingUnitDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(55)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(gebouwehid)), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(versieid)), new DbaseFieldLength(25)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(gebouwid)), new DbaseFieldLength(10)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(functie)), new DbaseFieldLength(30)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(20)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(posgeommet)), new DbaseFieldLength(30)),
            DbaseField.CreateLogicalField(new DbaseFieldName(nameof(afwijking)))
        };
    }
}
