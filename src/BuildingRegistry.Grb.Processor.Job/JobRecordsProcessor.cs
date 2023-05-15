namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Abstractions;
    using Api.BackOffice.Abstractions.Building;
    using Api.BackOffice.Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using NodaTime;

    public interface IJobRecordsProcessor
    {
        Task Process(IEnumerable<JobRecord> jobRecords, CancellationToken ct);
    }

    public sealed class JobRecordsProcessor : IJobRecordsProcessor
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly IBackOfficeApiProxy _backOfficeApiProxy;
        private readonly IErrorWarningEvaluator _errorWarningEvaluator;

        public JobRecordsProcessor(
            BuildingGrbContext buildingGrbContext,
            IBackOfficeApiProxy backOfficeApiProxy,
            IErrorWarningEvaluator errorWarningEvaluator)
        {
            _buildingGrbContext = buildingGrbContext;
            _backOfficeApiProxy = backOfficeApiProxy;
            _errorWarningEvaluator = errorWarningEvaluator;
        }

        public async Task Process(IEnumerable<JobRecord> jobRecords, CancellationToken ct)
        {
            foreach (var jobRecord in jobRecords.Where(x => x.Status == JobRecordStatus.Created))
            {
                BackOfficeApiResult backOfficeApiResult;

                switch (jobRecord.EventType)
                {
                    case GrbEventType.DefineBuilding:
                        backOfficeApiResult = await _backOfficeApiProxy.RealizeAndMeasureUnplannedBuilding(
                            new RealizeAndMeasureUnplannedBuildingRequest { GrbData = Map(jobRecord) }, ct);
                        break;
                    case GrbEventType.DemolishBuilding:
                        backOfficeApiResult = await _backOfficeApiProxy.DemolishBuilding(
                            jobRecord.GrId, new DemolishBuildingRequest { GrbData = Map(jobRecord) }, ct);
                        break;
                    case GrbEventType.MeasureBuilding:
                        backOfficeApiResult = await _backOfficeApiProxy.MeasureBuilding(
                            jobRecord.GrId, new MeasureBuildingRequest { GrbData = Map(jobRecord) }, ct);
                        break;
                    case GrbEventType.ChangeBuildingMeasurement:
                        backOfficeApiResult = await _backOfficeApiProxy.ChangeBuildingMeasurement(
                            jobRecord.GrId, new ChangeBuildingMeasurementRequest { GrbData = Map(jobRecord) }, ct);
                        break;
                    case GrbEventType.CorrectBuildingMeasurement:
                        backOfficeApiResult = await _backOfficeApiProxy.CorrectBuildingMeasurement(
                            jobRecord.GrId, new CorrectBuildingMeasurementRequest { GrbData = Map(jobRecord) }, ct);
                        break;
                    case GrbEventType.Unknown:
                    default:
                        throw new NotImplementedException($"Unsupported JobRecord EventType: {jobRecord.EventType}");
                }

                if (backOfficeApiResult.IsSuccess)
                {
                    jobRecord.TicketId = Guid.Parse(backOfficeApiResult.TicketUrl!.AsIdentifier().Map(x => x));
                    jobRecord.Status = JobRecordStatus.Pending;
                }
                else
                {
                    var evaluation = _errorWarningEvaluator.Evaluate(backOfficeApiResult.ValidationErrors);
                    jobRecord.Status = evaluation.jobRecordStatus;
                    jobRecord.ErrorMessage = evaluation.message;
                }

                await _buildingGrbContext.SaveChangesAsync(ct);
            }
        }

        private GrbData Map(JobRecord jobRecord)
        {
            return new GrbData
            {
                Idn = jobRecord.Idn,
                IdnVersion = jobRecord.IdnVersion,
                EndDate = jobRecord.EndDate == null
                    ? null
                    : new Rfc3339SerializableDateTimeOffset(Instant.FromDateTimeOffset(jobRecord.EndDate.Value)
                        .ToBelgianDateTimeOffset()).ToString(),
                VersionDate =
                    new Rfc3339SerializableDateTimeOffset(Instant.FromDateTimeOffset(jobRecord.VersionDate)
                        .ToBelgianDateTimeOffset()).ToString(),
                EventType = jobRecord.EventType.ToString(),
                Overlap = jobRecord.Overlap,
                GeometriePolygoon = GetGml(jobRecord.Geometry),
                GrbObject = jobRecord.GrbObject.ToString(),
                GrbObjectType = jobRecord.GrbObjectType.ToString()
            };
        }

        private static string GetGml(Geometry geometry)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };

            var polygon = geometry as Polygon;

            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Polygon", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
                WriteRing(polygon.ExteriorRing as LinearRing, xmlwriter);
                WriteInteriorRings(polygon.InteriorRings, polygon.NumInteriorRings, xmlwriter);
                xmlwriter.WriteEndElement();
            }

            return builder.ToString();
        }

        private static void WriteRing(LinearRing ring, XmlWriter writer, bool isInterior = false)
        {
            writer.WriteStartElement("gml", isInterior ? "interior" : "exterior", "http://www.opengis.net/gml/3.2");
            writer.WriteStartElement("gml", "LinearRing", "http://www.opengis.net/gml/3.2");
            writer.WriteStartElement("gml", "posList", "http://www.opengis.net/gml/3.2");

            var posListBuilder = new StringBuilder();
            foreach (var coordinate in ring.Coordinates)
            {
                posListBuilder.Append(string.Format(
                    Global.GetNfi(),
                    "{0} {1} ",
                    coordinate.X,
                    coordinate.Y));
            }

            //remove last space
            posListBuilder.Length--;

            writer.WriteValue(posListBuilder.ToString());

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WriteInteriorRings(LineString[] rings, int numInteriorRings, XmlWriter writer)
        {
            if (numInteriorRings < 1)
            {
                return;
            }

            foreach (var ring in rings)
            {
                WriteRing(ring as LinearRing, writer, true);
            }
        }
    }
}
