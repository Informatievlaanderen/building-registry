// namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
// {
//     using Abstractions.Building;
//     using Abstractions.Building.Requests;
//     using Abstractions.Building.SqsRequests;
//     using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
//     using BuildingRegistry.Building;
//     using BuildingRegistry.Building.Commands;
//
//     public sealed record MergeBuildingsLambdaRequest : BuildingLambdaRequest
//     {
//         public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
//         public MergeBuildingRequest Request { get; }
//
//         public MergeBuildingsLambdaRequest(string messageGroupId, MergeBuildingsSqsRequest sqsRequest)
//                 : base(
//                     messageGroupId,
//                     sqsRequest.TicketId,
//                     null,
//                     sqsRequest.ProvenanceData.ToProvenance(),
//                     sqsRequest.Metadata)
//         {
//             Request = sqsRequest.Request;
//             BuildingPersistentLocalId = new BuildingPersistentLocalId(sqsRequest.BuildingPersistentLocalId);
//         }
//
//         /// <summary>
//         /// Map command
//         /// </summary>
//         /// <returns>MergeBuildings.</returns>
//         public MergeBuildings ToCommand()
//         {
//             return new MergeBuildings(
//                 BuildingPersistentLocalId,
//                 Request.GeometriePolygoon.ToExtendedWkbGeometry(),
//                 Request.SamenvoegenGebouwen
//                     .Select(x => new BuildingPersistentLocalId(Convert.ToInt32(x.AsIdentifier().Map(s => s).Value))),
//                 Provenance);
//         }
//     }
// }
