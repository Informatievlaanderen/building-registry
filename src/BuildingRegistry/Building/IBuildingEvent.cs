namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public interface IBuildingEvent : IHasBuildingPersistentLocalId, IHasProvenance, ISetProvenance, IHaveHash, IMessage
    { }
}
