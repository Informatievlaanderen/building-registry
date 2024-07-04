namespace BuildingRegistry.Tests.Fixtures
{
    using System;
    using AutoFixture.Kernel;

    public class WithUniqueInteger : ISpecimenBuilder
    {
        private int _lastInt;

        public WithUniqueInteger(int? startPosition = null)
        {
            if (startPosition.HasValue)
                _lastInt = startPosition.Value;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is not Type type || type != typeof(int))
            {
                return new NoSpecimen();
            }

            var nextInt = _lastInt + 1;
            _lastInt = nextInt;

            return nextInt;
        }
    }
}
