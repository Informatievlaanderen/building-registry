using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda
{
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Building;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public class SqsBackOfficeHandlerFunction : FunctionBase
    {
        public override void ConfigureServices(ServiceCollection services)
        {
            base.ConfigureServices(services);
            // TODO: uncomment after initial lambda testing
            //services.AddTransient<IMessageHandler, MessageHandler>();
            //services.AddMediatR(typeof(SqsPlanBuildingHandler).Assembly);
        }
    }
}
