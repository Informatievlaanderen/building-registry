namespace BuildingRegistry.Api.Extract.Extracts
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.BuildingExtract;
    using System.Collections.Generic;
    using System.Linq;

    public class BuildingRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateBuildingFiles(ExtractContext context)
        {
            var extractItems = context
                .BuildingExtract
                .AsNoTracking()
                .Where(m => m.IsComplete && m.ShapeRecordContentLength > 0)
                .OrderBy(m => m.PersistentLocalId);

            yield return ExtractBuilder.CreateDbfFile<BuildingExtractItem, BuildingDbaseRecord>(
                ExtractFileNames.Building,
                new BuildingDbaseSchema(),
                extractItems,
                extractItems.Count,
                x => x.DbaseRecord);

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
                ExtractFileNames.Building,
                ShapeType.Polygon,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractFileNames.Building,
                ShapeType.Polygon,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);

            yield return ExtractBuilder.CreateProjectedCoordinateSystemFile(
                ExtractFileNames.Building,
                ProjectedCoordinateSystem.Belge_Lambert_1972);
        }
    }
}
