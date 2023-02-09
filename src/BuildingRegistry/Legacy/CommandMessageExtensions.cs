namespace BuildingRegistry.Legacy
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.CommandHandling;

    public static class CommandMessageExtensions
    {
        public static void AddMetadata(this CommandMessage commandMessage, string key, object value)
        {
            if (commandMessage.Metadata is not null)
            {
                if (commandMessage.Metadata.ContainsKey(key))
                {
                    throw new ArgumentException($"Element with key '{key}' already exists.");
                }

                commandMessage.Metadata.Add(key, value);
            }
        }

        public static void AddMetadata(this CommandMessage commandMessage, IDictionary<string, object> metadataDictionary)
        {
            foreach (var keyValuePair in metadataDictionary)
            {
                AddMetadata(commandMessage, keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
