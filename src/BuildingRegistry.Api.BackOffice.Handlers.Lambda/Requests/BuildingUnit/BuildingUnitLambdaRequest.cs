namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using System;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using BuildingRegistry.Building;

    public class BuildingUnitLambdaRequest : SqsLambdaRequest
    {
        public BuildingPersistentLocalId BuildingPersistentLocalId => new BuildingPersistentLocalId(Convert.ToInt32(MessageGroupId));
    }
}
