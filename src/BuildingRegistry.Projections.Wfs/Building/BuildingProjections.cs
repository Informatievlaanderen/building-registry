namespace BuildingRegistry.Projections.Wfs.Building
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("WFS gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het WFS gebouwenregister voorziet.")]
    public class BuildingProjections : ConnectedProjection<WfsContext>
    {
        private static readonly string RealizedStatus = GebouwStatus.Gerealiseerd.ToString();
        private static readonly string PlannedStatus = GebouwStatus.Gepland.ToString();
        private static readonly string RetiredStatus = GebouwStatus.Gehistoreerd.ToString();
        private static readonly string NotRealizedStatus = GebouwStatus.NietGerealiseerd.ToString();
        private static readonly string UnderConstructionStatus = GebouwStatus.InAanbouw.ToString();
        private static readonly string MeasuredMethod = GeometrieMethode.IngemetenGRB.ToString();
        private static readonly string OutlinedMethod = GeometrieMethode.Ingeschetst.ToString();

        private readonly WKBReader _wkbReader;

        public BuildingProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context.Buildings.AddAsync(new Building { BuildingId = message.Message.BuildingId }, cancellationToken: ct);
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.PersistentLocalId);
                building.PersistentLocalId = int.Parse(message.Message.PersistentLocalId.ToString());
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.IsComplete = true;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsComplete = false;

                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.IsRemoved = true;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = RealizedStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = RealizedStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = PlannedStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = PlannedStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = RetiredStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = RetiredStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = UnderConstructionStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = UnderConstructionStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = NotRealizedStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = NotRealizedStatus;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = null;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Status = null;
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                SetGeometry(building, message.Message.ExtendedWkbGeometry, MeasuredMethod);
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                SetGeometry(building, message.Message.ExtendedWkbGeometry, MeasuredMethod);
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                SetGeometry(building, message.Message.ExtendedWkbGeometry, OutlinedMethod);
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                SetGeometry(building, message.Message.ExtendedWkbGeometry, OutlinedMethod);
                SetVersion(building, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.Geometry = null;
                building.GeometryMethod = null;

                SetVersion(building, message.Message.Provenance.Timestamp);
            });
        }

        private static void SetVersion(Building building, Instant provenanceTimestamp)
        {
            building.Version = provenanceTimestamp;
        }

        private void SetGeometry(Building building, string extendedWkbGeometry, string method)
        {
            var geometry = _wkbReader.Read(extendedWkbGeometry.ToByteArray()) as Polygon;
            geometry = geometry == null ? null : new GrbPolygon(geometry);

            building.GeometryMethod = method;
            building.Geometry = geometry;
        }
    }
}
