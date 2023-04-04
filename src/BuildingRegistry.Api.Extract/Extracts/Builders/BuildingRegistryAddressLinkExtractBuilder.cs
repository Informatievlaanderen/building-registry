namespace BuildingRegistry.Api.Extract.Extracts.Builders
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.BuildingUnitAddressLinkExtract;

    public static class BuildingRegistryAddressLinkExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateBuildingUnitFiles(ExtractContext context)
        {
            var extractItems = context
                .BuildingUnitAddressLinkExtract
                .AsNoTracking();

            var projectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(BuildingUnitAddressLinkExtractProjections).FullName);

            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, projectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<BuildingUnitAddressLinkDbaseRecord>(
                ExtractFileNames.AddressLinkExtractZipName,
                new BuildingUnitAddressLinkDbaseSchema(),
                extractItems.OrderBy(m => m.BuildingUnitPersistentLocalId).Select(org => org.DbaseRecord),
                extractItems.Count);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.AddressLinkExtractZipName,
                extractMetadata);
        }
    }
}
