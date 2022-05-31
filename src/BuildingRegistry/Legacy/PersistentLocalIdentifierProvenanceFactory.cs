namespace BuildingRegistry.Legacy
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands.Crab;
    using NodaTime;

    public class PersistentLocalIdentifierProvenanceFactory : IProvenanceFactory<Building>
    {
        private static readonly List<Type> AllowedTypes = new List<Type>
        {
            typeof(AssignPersistentLocalIdForCrabTerrainObjectId),
            typeof(RequestPersistentLocalIdsForCrabTerrainObjectId),
        };

        private static bool CanCreateFrom(Type? type) => type != null && AllowedTypes.Contains(type);

        public bool CanCreateFrom<TCommand>() => CanCreateFrom(typeof(TCommand));

        public Provenance CreateFrom(object command, Building aggregate)
        {
            var commandType = command?.GetType();
            if (CanCreateFrom(commandType))
                return new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.BuildingRegistry,
                    new Reason("Stabiele en unieke objectidentificatie"),
                    new Operator("BuildingRegistry"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen);

            throw new ApplicationException($"Cannot create provenance for {commandType?.Name}");
        }
    }
}
