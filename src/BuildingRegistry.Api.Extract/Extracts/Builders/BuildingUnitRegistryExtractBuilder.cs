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

    public static class BuildingUnitRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateBuildingUnitFiles(ExtractContext context)
        {
            var extractItems = context
                .BuildingUnitExtract
                .AsNoTracking()
                .Where(m => m.IsComplete && m.IsBuildingComplete && m.ShapeRecordContentLength > 0)
                .OrderBy(m => m.PersistentLocalId);

            var buildingUnitProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(BuildingUnitExtractProjections).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, buildingUnitProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<BuildingUnitExtractItem, BuildingUnitDbaseRecord>(
                ExtractFileNames.BuildingUnitExtractZipName,
                new BuildingUnitDbaseSchema(),
                extractItems,
                extractItems.Count,
                x => x.DbaseRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.BuildingUnitExtractZipName,
                extractMetadata);

            var boundingBox = new BoundingBox3D(
                extractItems.Where(x => x.MinimumX > 0).Min(record => record.MinimumX),
                extractItems.Where(x => x.MinimumY > 0).Min(record => record.MinimumY),
                extractItems.Where(x => x.MaximumX > 0).Max(record => record.MaximumX),
                extractItems.Where(x => x.MaximumY > 0).Max(record => record.MaximumY),
                0,
                0,
                double.NegativeInfinity,
                double.PositiveInfinity);

            yield return ExtractBuilder.CreateShapeFile<PointShapeContent>(
                ExtractFileNames.BuildingUnitExtractZipName,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractFileNames.BuildingUnitExtractZipName,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);

            yield return ExtractBuilder.CreateProjectedCoordinateSystemFile(
                ExtractFileNames.BuildingUnitExtractZipName,
                ProjectedCoordinateSystem.Belge_Lambert_1972);
        }
    }
}
