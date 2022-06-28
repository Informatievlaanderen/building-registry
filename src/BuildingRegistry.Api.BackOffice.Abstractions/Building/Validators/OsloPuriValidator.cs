namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;

    public static class OsloPuriValidator
    {
        public static bool TryParseIdentifier(string url, out string identifier)
        {
            identifier = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }

                identifier = url
                    .AsIdentifier()
                    .Map(x => x);

                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static bool TryParsePersistentLocalId(string url, out int persistentLocalId)
        {
            persistentLocalId = 0;
            return TryParseIdentifier(url, out var stringId) && int.TryParse(stringId, out persistentLocalId);
        }


        /// <exception cref="InvalidOperationException"></exception>
        public static int ParsePersistentLocalId(string url)
        {
            if (TryParseIdentifier(url, out var stringId) && int.TryParse(stringId, out int persistentLocalId))
            {
                return persistentLocalId;
            };

            throw new InvalidOperationException();
        }
    }
}
