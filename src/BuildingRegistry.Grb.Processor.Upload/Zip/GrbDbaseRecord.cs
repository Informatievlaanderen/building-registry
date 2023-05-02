namespace BuildingRegistry.Grb.Processor.Upload.Zip
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GrbDbaseRecord : DbaseRecord
    {
        public static readonly GrbDbaseSchema Schema = new GrbDbaseSchema();

        public DbaseInt32 IDN { get; }
        public DbaseInt32 IDNV { get; }
        public DbaseDate GVDV { get; }
        public DbaseDate GVDE { get; }
        public DbaseInt32 EventType { get; }
        public DbaseInt32 GRBOBJECT { get; }
        public DbaseCharacter GRID { get; }
        public DbaseInt32 TPC { get; }

        public GrbDbaseRecord()
        {
            IDN = new DbaseInt32(Schema.IDN);
            IDNV = new DbaseInt32(Schema.IDNV);
            GVDV = new DbaseDate(Schema.GVDV);
            GVDE = new DbaseDate(Schema.GVDE);
            EventType = new DbaseInt32(Schema.EventType);
            GRBOBJECT = new DbaseInt32(Schema.GRBOBJECT);
            GRID = new DbaseCharacter(Schema.GRID);
            TPC = new DbaseInt32(Schema.TPC);

            Values = new DbaseFieldValue[]
            {
                IDN,
                IDNV,
                GVDV,
                GVDE,
                EventType,
                GRBOBJECT,
                GRID,
                TPC
            };
        }
    }
}
