namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    public interface IHasBackOfficeRequest<out TBackOfficeRequest>
    {
        public TBackOfficeRequest Request { get; }
    }
}
