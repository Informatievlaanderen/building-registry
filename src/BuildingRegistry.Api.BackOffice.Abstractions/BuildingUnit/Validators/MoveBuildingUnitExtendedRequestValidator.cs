namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Building.Validators;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Requests;
    using Validation;

    public record MoveBuildingUnitExtendedRequest(MoveBuildingUnitRequest Request, int BuildingUnitPersistentLocalId);

    public class MoveBuildingUnitExtendedRequestValidator : AbstractValidator<MoveBuildingUnitExtendedRequest>
    {
        public MoveBuildingUnitExtendedRequestValidator(
            BuildingExistsValidator buildingExistsValidator,
            BackOfficeContext backOfficeContext)
        {
            RuleFor(x => x.Request.DoelgebouwId)
                .Must(ValidateBuildingPuri)
                .DependentRules(() =>
                {
                    RuleFor(x => x.Request.DoelgebouwId)
                        .MustAsync(async (x, ct) => await ValidateBuildingExists(x, buildingExistsValidator, ct))
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.BuildingUnitPersistentLocalId)
                                .MustAsync(async (request, buildingUnitPersistentLocalId, ct) =>
                                {
                                    var relationWithSourceBuilding = await backOfficeContext
                                        .FindBuildingUnitBuildingRelation(new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId), ct);

                                    if (relationWithSourceBuilding is null)
                                    {
                                        throw new ApiException(ValidationErrors.Common.BuildingUnitNotFound.Message, StatusCodes.Status404NotFound);
                                    }

                                    var destinationBuildingPersistentLocalId = int.Parse(request.Request.DoelgebouwId.AsIdentifier().Map(x => x));

                                    return destinationBuildingPersistentLocalId != relationWithSourceBuilding.BuildingPersistentLocalId;
                                })
                                .WithErrorCode(ValidationErrors.MoveBuildingUnit.SourceAndDestinationBuildingAreTheSame.Code)
                                .WithMessage(x =>
                                    ValidationErrors.MoveBuildingUnit.SourceAndDestinationBuildingAreTheSame.Message(x.Request.DoelgebouwId))
                                .OverridePropertyName(nameof(MoveBuildingUnitRequest.DoelgebouwId));
                        })
                        .WithErrorCode(ValidationErrors.MoveBuildingUnit.BuildingNotFound.Code)
                        .WithMessage((_, puri) => ValidationErrors.MoveBuildingUnit.BuildingNotFound.MessageWithPuri(puri))
                        .OverridePropertyName(nameof(MoveBuildingUnitRequest.DoelgebouwId));
                })
                .WithErrorCode(ValidationErrors.Common.BuildingIdInvalid.Code)
                .WithMessage(ValidationErrors.Common.BuildingIdInvalid.Message)
                .OverridePropertyName(nameof(MoveBuildingUnitRequest.DoelgebouwId));
        }

        private static bool ValidateBuildingPuri(string puri)
        {
            return OsloPuriValidator.TryParseIdentifier(puri, out var id) && int.TryParse(id, out _);
        }

        private static async Task<bool> ValidateBuildingExists(
            string puri,
            BuildingExistsValidator buildingExistsValidator,
            CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = int.Parse(puri.AsIdentifier().Map(x => x));

            return await buildingExistsValidator.Exists(new BuildingPersistentLocalId(buildingPersistentLocalId), cancellationToken);
        }
    }
}
