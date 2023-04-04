namespace BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public sealed class BuildingUnitAddressLinkDbaseRecord : DbaseRecord
    {
        public static readonly BuildingUnitAddressLinkDbaseSchema Schema = new BuildingUnitAddressLinkDbaseSchema();

        public DbaseCharacter objecttype { get; }
        public DbaseCharacter adresobjid { get; }
        public DbaseInt32 adresid { get; }

        public BuildingUnitAddressLinkDbaseRecord()
        {
            objecttype = new DbaseCharacter(Schema.objecttype);
            adresobjid = new DbaseCharacter(Schema.adresobjid);
            adresid = new DbaseInt32(Schema.adresid);

            Values = new DbaseFieldValue[]
            {
                objecttype,
                adresobjid,
                adresid
            };
        }
    }
}
