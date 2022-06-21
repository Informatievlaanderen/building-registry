using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs.Lambda
{
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public class SqsPostHandlerFunction : FunctionBase
    {
        public override void ConfigureServices(ServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddTransient<IMessageHandler, MessageHandler>();
            services.AddMediatR(typeof(SqsPostHandler).Assembly);
        }
    }
}
