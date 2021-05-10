namespace BuildingRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using NodaTime;

    public class BuildingProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Building.Building>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCrabProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder,
            Building.Building aggregate)
        {
            if (!(provenanceHolder is IHasCrabProvenance crabProvenance))
                throw new ApplicationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");

            return CreateFrom(
                aggregate.LastModificationBasedOnCrab,
                crabProvenance.Timestamp,
                crabProvenance.Modification,
                crabProvenance.Operator,
                crabProvenance.Organisation);
        }
    }

    public class FixGrar1359ProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Building.Building>
    {
        public bool CanCreateFrom<TCommand>() => typeof(FixGrar1359).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder,
            Building.Building aggregate)
        {
            if (!(provenanceHolder is FixGrar1359 crabProvenance))
                throw new ApplicationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");

            return new Provenance(Instant.FromDateTimeUtc(DateTime.UtcNow), Application.Unknown, new Reason("Rechtzetting gebouweenheden"), new Operator("crabadmin"), Modification.Update, Organisation.DigitaalVlaanderen);
        }
    }
}

