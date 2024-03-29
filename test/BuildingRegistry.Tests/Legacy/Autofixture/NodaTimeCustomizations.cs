namespace BuildingRegistry.Tests.Legacy.Autofixture
{
    using System;
    using AutoFixture;
    using AutoFixture.Kernel;
    using NodaTime;

    public class InfrastructureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize(new NodaTimeCustomization());
            fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
        }
    }

    public class NodaTimeCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new LocalDateGenerator());
            fixture.Customizations.Add(new LocalTimeGenerator());
            fixture.Customizations.Add(new LocalDateTimeGenerator());
            fixture.Customizations.Add(new InstantGenerator());
        }

        public class InstantGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(Instant).Equals(request))
                {
                    return new NoSpecimen();
                }

                return Instant.FromDateTimeOffset(DateTimeOffset.Now);
            }
        }

        public class LocalDateGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalDate).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalDate.FromDateTime(DateTime.Today);
            }
        }

        public class LocalTimeGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalTime).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalTime.FromTicksSinceMidnight(DateTime.Now.TimeOfDay.Ticks);
            }
        }

        public class LocalDateTimeGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalDateTime).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalDateTime.FromDateTime(DateTime.Now);
            }
        }
    }
}
