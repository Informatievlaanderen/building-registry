namespace BuildingRegistry.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using AutoFixture.Kernel;

    public class RandomUniqueIntegerGenerator : ISpecimenBuilder
    {
        private readonly Random _random = new Random();

        private readonly HashSet<int> _generatedIntegers = new HashSet<int>();

        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(int))
            {
                int nextInt;
                do
                {
                    nextInt = _random.Next();
                } while (_generatedIntegers.Contains(nextInt));
                _generatedIntegers.Add(nextInt);
                return nextInt;
            }

            return new NoSpecimen();
        }
    }
}
