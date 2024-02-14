namespace BuildingRegistry.Api.Extract.Extracts.Builders
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Projections.Extract;
    using BuildingRegistry.Projections.Extract.BuildingExtract;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingRegistryExtractV2EsriBuilder
    {
        public static IEnumerable<ExtractFile> CreateBuildingFiles(ExtractContext context)
        {
            var extractItems = context
                .BuildingExtractV2Esri
                .AsNoTracking()
                .Where(m => m.ShapeRecordContentLength > 0)
                .OrderBy(m => m.PersistentLocalId);

            var buildingProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(BuildingExtractV2EsriProjections).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, buildingProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<BuildingExtractItemV2Esri, BuildingDbaseRecord>(
                ExtractFileNames.BuildingExtractZipName,
                new BuildingDbaseSchema(),
                extractItems,
                extractItems.Count,
                x => x.DbaseRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.BuildingExtractZipName,
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

            yield return ExtractBuilder.CreateShapeFile<PolygonShapeContent>(
                ExtractFileNames.BuildingExtractZipName,
                ShapeType.Polygon,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractFileNames.BuildingExtractZipName,
                ShapeType.Polygon,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);

            yield return ExtractBuilder.CreateProjectedCoordinateSystemFile(
                ExtractFileNames.BuildingExtractZipName,
                ProjectedCoordinateSystem.Belge_Lambert_1972);
        }
    }
}
