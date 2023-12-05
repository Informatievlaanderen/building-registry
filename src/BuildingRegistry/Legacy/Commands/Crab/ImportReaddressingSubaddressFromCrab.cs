namespace BuildingRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Legacy.Crab;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class ImportReaddressingSubaddressFromCrab
    {
        private static readonly Guid Namespace = new Guid("cbb6858f-cc19-49e1-ac79-29d80593b170");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabReaddressingId ReaddressingId { get; }
        public ReaddressingBeginDate BeginDate { get; }

        public CrabSubaddressId OldSubaddressId { get; }
        public CrabAddressNature OldAddressNature { get; }
        public CrabTerrainObjectHouseNumberId OldTerrainObjectHouseNumberId { get; }

        public CrabSubaddressId NewSubaddressId { get; }
        public CrabAddressNature NewAddressNature { get; }
        public CrabTerrainObjectHouseNumberId NewTerrainObjectHouseNumberId { get; }

        public ImportReaddressingSubaddressFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabReaddressingId readdressingId,
            ReaddressingBeginDate beginDate,
            CrabSubaddressId oldSubaddressId,
            CrabAddressNature oldAddressNature,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabSubaddressId newSubaddressId,
            CrabAddressNature newAddressNature,
            CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId)
        {
            TerrainObjectId = terrainObjectId;
            ReaddressingId = readdressingId;
            BeginDate = beginDate;
            OldSubaddressId = oldSubaddressId;
            OldAddressNature = oldAddressNature;
            OldTerrainObjectHouseNumberId = oldTerrainObjectHouseNumberId;
            NewSubaddressId = newSubaddressId;
            NewAddressNature = newAddressNature;
            NewTerrainObjectHouseNumberId = newTerrainObjectHouseNumberId;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"ImportReaddressingSubaddressFromCrab-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return ReaddressingId;
            yield return BeginDate;
            yield return OldSubaddressId;
            yield return OldAddressNature;
            yield return OldTerrainObjectHouseNumberId;
            yield return NewSubaddressId;
            yield return NewAddressNature;
            yield return NewTerrainObjectHouseNumberId;
        }
    }
}
