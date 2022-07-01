namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.Threading.Tasks;

    public class Ticketing : ITicketing
    {
        public Task<Guid> CreateTicket() => Task.FromResult(Guid.NewGuid());

        public Task Pending(Guid ticketId) => Task.CompletedTask;

        public Task Complete(Guid ticketId, object ticketStatus) => Task.CompletedTask;

        public Task<object> Get(Guid ticketId) => Task.FromResult(new object());
    }
}
