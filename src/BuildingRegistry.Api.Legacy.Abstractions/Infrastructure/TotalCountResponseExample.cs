namespace BuildingRegistry.Api.Legacy.Abstractions.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Swashbuckle.AspNetCore.Filters;

    public class TotalCountResponseExample : IExamplesProvider<TotaalAantalResponse>
    {
        public TotaalAantalResponse GetExamples()
        {
            return new TotaalAantalResponse
            {
                Aantal = 574512
            };
        }
    }
}