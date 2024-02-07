namespace BuildingRegistry.Projections.Integration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;

    public sealed class MunicipalityGeometry
    {
        public string NisCode { get; set; }
        public Geometry Geometry { get; set; }

        public MunicipalityGeometry()
        { }
    }

    public sealed class MunicipalityGeometryConfiguration : IEntityTypeConfiguration<MunicipalityGeometry>
    {
        public void Configure(EntityTypeBuilder<MunicipalityGeometry> builder)
        {
            const string schema = "integration_municipality";
            const string viewName = "municipality_geometries";

            builder.Property(x => x.NisCode).HasColumnName("nis_code");
            builder.Property(x => x.Geometry).HasColumnName("geometry");

            builder
                .ToView(viewName, schema) // It's actually a table, but to not create EF migrations, we configure it as a view without a key.
                .HasNoKey()
                .ToSqlQuery($"SELECT nis_code, geometry FROM {schema}.{viewName}");
        }
    }
}
