namespace BuildingRegistry.Api.Extract.Extracts
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using System.Collections.Generic;
    using System.Linq;
    using Projections.Extract.BuildingUnitExtract;

    public class BuildingUnitRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateBuildingUnitFiles(ExtractContext context)
        {
            var extractItems = context
                .BuildingUnitExtract
                .AsNoTracking()
                .Where(m => m.IsComplete && m.IsBuildingComplete && m.ShapeRecordContentLength > 0)
                .OrderBy(m => m.PersistentLocalId);

            yield return ExtractBuilder.CreateDbfFile<BuildingUnitExtractItem, BuildingUnitDbaseRecord>(
                ExtractController.BuildingUnitZipName,
                new BuildingUnitDbaseSchema(),
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

            yield return ExtractBuilder.CreateShapeFile<PointShapeContent>(
                ExtractController.BuildingUnitZipName,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractController.BuildingUnitZipName,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);
        }
    }
}
