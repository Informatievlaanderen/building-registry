namespace BuildingRegistry.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CollectionExtensions
    {
        public static IEnumerable<ICollection<T>> SplitIntoBatches<T>(this ICollection<T> collection, int batchCount)
        {
            var batchSize = (double)collection.Count / batchCount;
            var batchSizeRounded = (int)Math.Floor(batchSize);
            if (batchSize != batchSizeRounded)
            {
                batchSizeRounded++;
            }
            return Enumerable.Range(0, batchCount).Select(batchIndex => collection.Skip(batchSizeRounded * batchIndex).Take(batchSizeRounded).ToArray());
        }
    }
}
