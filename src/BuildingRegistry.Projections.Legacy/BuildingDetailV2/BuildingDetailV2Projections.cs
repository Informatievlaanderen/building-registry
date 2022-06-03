namespace BuildingRegistry.Projections.Legacy.BuildingDetailV2
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;

    [ConnectedProjectionName("API endpoint detail/lijst gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het gebouwen detail & lijst voorziet.")]
    public class BuildingDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public BuildingDetailV2Projections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var building = new BuildingDetailItemV2(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Parse(message.Message.GeometryMethod),
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingStatus.Parse(message.Message.BuildingStatus),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                await context
                    .BuildingDetailsV2
                    .AddAsync(building, ct);
            });
        }
    }
}
