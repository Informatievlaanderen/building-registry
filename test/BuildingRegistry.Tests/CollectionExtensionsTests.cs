namespace BuildingRegistry.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Xunit;

    public class CollectionExtensionsTests
    {
        [Theory]
        [MemberData(nameof(SplitIntoBatchesCases))]
        public void SplitIntoBatches(int collectionCount, int batchCount, int[] expectedBatchSizes)
        {
            var collection = Enumerable.Range(1, collectionCount).ToArray();
            var batches = collection.SplitIntoBatches(batchCount).ToArray();
            Assert.Equal(expectedBatchSizes.Length, batches.Length);
            for (var i = 0; i < batches.Length; i++)
            {
                Assert.Equal(expectedBatchSizes[i], batches[i].Count);
            }
        }

        public static IEnumerable<object[]> SplitIntoBatchesCases()
        {
            yield return new object[] { 1, 1, new[] { 1 } };
            yield return new object[] { 2, 1, new[] { 2 } };
            yield return new object[] { 1, 2, new[] { 1, 0 } };
            yield return new object[] { 2, 2, new[] { 1, 1 } };
            yield return new object[] { 3, 2, new[] { 2, 1 } };
        }
    }
}
