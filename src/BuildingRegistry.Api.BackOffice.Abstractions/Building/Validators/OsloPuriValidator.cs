namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;

    public static class OsloPuriValidatorExtensions
    {
        public static bool TryParsePersistentLocalId(string url, out int persistentLocalId)
        {
            persistentLocalId = 0;
            return OsloPuriValidator.TryParseIdentifier(url, out var stringId) && int.TryParse(stringId, out persistentLocalId);
        }

        /// <exception cref="InvalidOperationException"></exception>
        public static int ParsePersistentLocalId(string url)
        {
            if (OsloPuriValidator.TryParseIdentifier(url, out var stringId) && int.TryParse(stringId, out int persistentLocalId))
            {
                return persistentLocalId;
            };

            throw new InvalidOperationException();
        }
    }
}
