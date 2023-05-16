namespace BuildingRegistry.Tests.Grb.UploadProcessor;

using Xunit;

[CollectionDefinition(COLLECTION)]
public class ZipArchiveCollectionFixture : ICollectionFixture<ZipArchiveFixture>
{
    public const string COLLECTION = "ZipArchiveOpener";
}
