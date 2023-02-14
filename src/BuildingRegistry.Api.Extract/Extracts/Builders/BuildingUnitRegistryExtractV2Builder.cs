namespace BuildingRegistry.Api.Extract.Extracts.Builders
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Projections.Extract;
    using BuildingRegistry.Projections.Extract.BuildingUnitExtract;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingUnitRegistryExtractV2Builder
    {
        public static IEnumerable<ExtractFile> CreateBuildingUnitFiles(ExtractContext context)
        {
            var extractItems = context
                .BuildingUnitExtractV2
                .AsNoTracking()
                .Where(m => m.ShapeRecordContentLength > 0)
                .OrderBy(m => m.BuildingUnitPersistentLocalId);

            var buildingUnitProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(BuildingUnitExtractV2Projections).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, buildingUnitProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<BuildingUnitExtractItemV2, BuildingUnitDbaseRecord>(
                ExtractFileNames.BuildingUnit,
                new BuildingUnitDbaseSchema(),
                extractItems,
                extractItems.Count,
                x => x.DbaseRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.BuildingUnit,
                extractMetadata);

            var anyItems = extractItems.Any();
            var boundingBox = new BoundingBox3D(
                anyItems ? extractItems.Where(x => x.MinimumX > 0).Min(record => record.MinimumX) : 0,
                anyItems ? extractItems.Where(x => x.MinimumY > 0).Min(record => record.MinimumY) : 0,
                anyItems ? extractItems.Where(x => x.MaximumX > 0).Max(record => record.MaximumX) : 0,
                anyItems ? extractItems.Where(x => x.MaximumY > 0).Max(record => record.MaximumY) : 0,
                0,
                0,
                double.NegativeInfinity,
                double.PositiveInfinity);

            yield return ExtractBuilder.CreateShapeFile<PointShapeContent>(
                ExtractFileNames.BuildingUnit,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractFileNames.BuildingUnit,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);

            yield return ExtractBuilder.CreateProjectedCoordinateSystemFile(
                ExtractFileNames.BuildingUnit,
                ProjectedCoordinateSystem.Belge_Lambert_1972);
        }
    }
}
