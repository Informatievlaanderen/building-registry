namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands.Crab;
    using NodaTime;

    public class BuildingProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Building>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCrabProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Building aggregate)
        {
            if (provenanceHolder is not IHasCrabProvenance crabProvenance)
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return CreateFrom(
                aggregate.LastModificationBasedOnCrab,
                crabProvenance.Timestamp,
                crabProvenance.Modification,
                crabProvenance.Operator,
                crabProvenance.Organisation);
        }
    }

    public class BuildingLegacyProvenanceFactory : IProvenanceFactory<Building>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCommandProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Building aggregate)
        {
            if (provenanceHolder is not IHasCommandProvenance provenance)
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return provenance.Provenance;
        }
    }


    public class FixGrar1359ProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Building>
    {
        public bool CanCreateFrom<TCommand>() => typeof(FixGrar1359).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Building aggregate)
        {
            if (provenanceHolder is not FixGrar1359)
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return new Provenance(Instant.FromDateTimeUtc(DateTime.UtcNow), Application.Unknown, new Reason("Rechtzetting gebouweenheden"), new Operator("crabadmin"), Modification.Update, Organisation.DigitaalVlaanderen);
        }
    }
}

