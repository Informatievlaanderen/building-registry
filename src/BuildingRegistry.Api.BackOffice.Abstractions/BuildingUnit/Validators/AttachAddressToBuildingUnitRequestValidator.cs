namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Building.Validators;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Requests;
    using Validation;

    public class AttachAddressToBuildingUnitRequestValidator : AbstractValidator<AttachAddressToBuildingUnitRequest>
    {
        public AttachAddressToBuildingUnitRequestValidator(IAddresses addresses)
        {
            RuleFor(x => x.AdresId)
                .Must(adresId =>
                    OsloPuriValidator.TryParseIdentifier(adresId, out var id)
                    && int.TryParse(id, out _))
                .DependentRules(() =>
                {
                    RuleFor(x => x.AdresId)
                        .Must(adresId =>
                        {
                            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(adresId);

                            var address = addresses.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                            return address is not null && !address.Value.IsRemoved;
                        }).DependentRules(() =>
                        {
                            RuleFor(x => x.AdresId)
                                .Must(adresId =>
                                {
                                    var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(adresId);

                                    var address = addresses.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                                    return address.Value.Status == BuildingRegistry.Building.Datastructures.AddressStatus.Current
                                           || address.Value.Status == BuildingRegistry.Building.Datastructures.AddressStatus.Proposed;
                                })
                                .WithErrorCode(ValidationErrors.AttachAddressToBuildingUnit.AddressInvalidStatus.Code)
                                .WithMessage(ValidationErrors.AttachAddressToBuildingUnit.AddressInvalidStatus.Message);
                        })
                        .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code)
                        .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message);
                })
                .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message)
                .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code);
        }
    }
}
