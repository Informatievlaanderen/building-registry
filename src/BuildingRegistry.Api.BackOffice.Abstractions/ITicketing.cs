namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.Threading.Tasks;

    public interface ITicketing
    {
        Task<Guid> CreateTicket();
        Task Pending(Guid ticketId);
        Task Complete(Guid ticketId, object ticketStatus);
        Task<object> Get(Guid ticketId);
    }
}
