namespace BuildingRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Legacy.Crab;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class ImportReaddressingHouseNumberFromCrab
    {
        private static readonly Guid Namespace = new Guid("d68f43de-d158-498b-a438-c66d7394eb3e");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabReaddressingId ReaddressingId { get; }
        public ReaddressingBeginDate BeginDate { get; }

        public CrabHouseNumberId OldHouseNumberId { get; }
        public CrabAddressNature OldAddressNature { get; }
        public CrabTerrainObjectHouseNumberId OldTerrainObjectHouseNumberId { get; }

        public CrabHouseNumberId NewHouseNumberId { get; }
        public CrabAddressNature NewAddressNature { get; }
        public CrabTerrainObjectHouseNumberId NewTerrainObjectHouseNumberId { get; }

        public ImportReaddressingHouseNumberFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabReaddressingId readdressingId,
            ReaddressingBeginDate beginDate,
            CrabHouseNumberId oldHouseNumberId,
            CrabAddressNature oldAddressNature,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabHouseNumberId newHouseNumberId,
            CrabAddressNature newAddressNature,
            CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId)
        {
            TerrainObjectId = terrainObjectId;
            ReaddressingId = readdressingId;
            BeginDate = beginDate;
            OldHouseNumberId = oldHouseNumberId;
            OldAddressNature = oldAddressNature;
            OldTerrainObjectHouseNumberId = oldTerrainObjectHouseNumberId;
            NewHouseNumberId = newHouseNumberId;
            NewAddressNature = newAddressNature;
            NewTerrainObjectHouseNumberId = newTerrainObjectHouseNumberId;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"ImportReaddressingHouseNumberFromCrab-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return ReaddressingId;
            yield return BeginDate;
            yield return OldHouseNumberId;
            yield return OldAddressNature;
            yield return OldTerrainObjectHouseNumberId;
            yield return NewHouseNumberId;
            yield return NewAddressNature;
            yield return NewTerrainObjectHouseNumberId;
        }
    }
}
