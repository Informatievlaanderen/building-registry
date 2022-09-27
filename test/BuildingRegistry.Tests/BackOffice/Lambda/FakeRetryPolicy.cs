namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;

    internal class FakeRetryPolicy : ICustomRetryPolicy
    {
        public Task Retry(Func<Task> functionToRetry)
        {
            return functionToRetry();
        }
    }
}
