namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Building.Validators;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Requests;
    using Validation;

    public class DetachAddressFromBuildingUnitRequestValidator : AbstractValidator<DetachAddressFromBuildingUnitRequest>
    {
        public DetachAddressFromBuildingUnitRequestValidator(IAddresses addresses)
        {
            RuleFor(x => x.AdresId)
                .Must(adresId => OsloPuriValidator.TryParseIdentifier(adresId, out var id) && int.TryParse(id, out _))
                .DependentRules(() =>
                {
                    RuleFor(x => x.AdresId)
                        .Must(adresId =>
                        {
                            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(adresId);
                            var address = addresses.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                            return address is not null && !address.Value.IsRemoved;
                        })
                        .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code)
                        .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message);
                })
                .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message)
                .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code);
        }
    }
}
