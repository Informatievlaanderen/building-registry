namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitRemoval
        {
            public static class InvalidBuildingStatus
            {
                public const string Code = "GebouweenheidVerwijderingOngedaanmakenGebouwIdNietGerealiseerdofGehistoreerd";
                public const string Message = "Verwijdering van gebouweenheid kan niet ongedaan gemaakt worden omdat gebouw status nietGerealiseerd of gehistoreerd heeft. Gebouw dient status gepland, inAanbouw of gerealiseerd te hebben.";
            }
        }
    }
}
