namespace BuildingRegistry.Producer.Ldes
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using BuildingRegistry.Infrastructure;

    public class ProducerContext : RunnerDbContext<ProducerContext>
    {
        public override string ProjectionStateSchema => Schema.ProducerLdes;

        public DbSet<BuildingDetail> Buildings => Set<BuildingDetail>();
        public DbSet<BuildingUnitDetail> BuildingUnits => Set<BuildingUnitDetail>();

        // This needs to be here to please EF
        public ProducerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ProducerContext(DbContextOptions<ProducerContext> options)
            : base(options) { }
    }
}
