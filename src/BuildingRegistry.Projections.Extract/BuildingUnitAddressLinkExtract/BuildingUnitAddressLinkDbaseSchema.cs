namespace BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public sealed class BuildingUnitAddressLinkDbaseSchema : DbaseSchema
    {
        public DbaseField objecttype => Fields[0];
        public DbaseField adresobjid => Fields[1];
        public DbaseField adresid => Fields[2];

        public BuildingUnitAddressLinkDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(objecttype)), new DbaseFieldLength(20)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(adresobjid)), new DbaseFieldLength(30)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(adresid)), new DbaseFieldLength(DbaseInt32.MaximumIntegerDigits.ToInt32()), new DbaseDecimalCount(0))
        };
    }
}

