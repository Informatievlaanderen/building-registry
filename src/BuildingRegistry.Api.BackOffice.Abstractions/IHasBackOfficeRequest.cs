namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    public interface IHasBackOfficeRequest<TBackOfficeRequest>
    {
        public TBackOfficeRequest Request { get; set; }
    }
}
