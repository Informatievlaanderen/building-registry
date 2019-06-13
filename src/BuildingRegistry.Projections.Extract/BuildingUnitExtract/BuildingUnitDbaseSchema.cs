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

        public BuildingUnitDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateStringField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(55)),
            DbaseField.CreateInt32Field(new DbaseFieldName(nameof(gebouwehid)), new DbaseFieldLength(10)),
            DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(versieid))),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(gebouwid)), new DbaseFieldLength(10)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(functie)), new DbaseFieldLength(30)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(20)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(posgeommet)), new DbaseFieldLength(30))
        };
    }
}
