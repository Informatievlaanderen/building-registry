namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs.Lambda.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Amazon.Lambda.Serialization.Json;
    using Amazon.Lambda.SQSEvents;
    using Amazon.Lambda.TestUtilities;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Lambda;
    using Xunit;

    public class SqsPostHandlerFunctionTest
    {
        [Fact]
        public async Task TestCrabImportLambdaFunction()
        {
            var sqsPostRequest = new SqsPostRequest(new List<RegisterCrabImportRequest[]> { new []
            {
                new RegisterCrabImportRequest(nameof(TestCrabImportLambdaFunction), nameof(SqsPostHandlerFunctionTest))
            } }, new Dictionary<string, object>(), new IdempotentCommandHandlerModule(new ContainerBuilder().Build()));

            var serializer = new JsonSerializer();

            var sqsJsonMessage = SqsJsonMessage.Create(sqsPostRequest, serializer);
            var serialized = serializer.Serialize(sqsJsonMessage);

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = serialized
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new SqsPostHandlerFunction();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains(serialized, logger.Buffer.ToString());
        }
    }
}
