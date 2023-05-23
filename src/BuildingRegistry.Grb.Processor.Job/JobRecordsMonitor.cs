namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Newtonsoft.Json;
    using TicketingService.Abstractions;

    public interface IJobRecordsMonitor
    {
        Task Monitor(Guid jobId, CancellationToken ct);
    }

    public class JobRecordsMonitor : IJobRecordsMonitor
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly ITicketing _ticketing;

        public JobRecordsMonitor(
            BuildingGrbContext buildingGrbContext,
            ITicketing ticketing)
        {
            _buildingGrbContext = buildingGrbContext;
            _ticketing = ticketing;
        }

        public async Task Monitor(Guid jobId, CancellationToken ct)
        {
            var pendingJobRecords = _buildingGrbContext.JobRecords.Where(x =>
                x.JobId == jobId
                && x.Status == JobRecordStatus.Pending).ToList();

            while (pendingJobRecords.Any())
            {
                await Parallel.ForEachAsync(
                    pendingJobRecords,
                    new ParallelOptions { MaxDegreeOfParallelism = 10 },
                    async (jobRecord, innerCt) =>
                    {
                        var ticket = await _ticketing.Get(jobRecord.TicketId!.Value, innerCt);

                        switch (ticket!.Status)
                        {
                            case TicketStatus.Created:
                            case TicketStatus.Pending:
                                break;
                            case TicketStatus.Complete:
                                var etagResponse = JsonConvert.DeserializeObject<ETagResponse>(ticket.Result!.ResultAsJson!);
                                jobRecord.BuildingPersistentLocalId = etagResponse!.Location.AsIdentifier().Map(int.Parse);
                                jobRecord.Status = JobRecordStatus.Complete;
                                break;
                            case TicketStatus.Error:
                                var ticketError = JsonConvert.DeserializeObject<TicketError>(ticket.Result!.ResultAsJson!);
                                var evaluation = ErrorWarningEvaluator.Evaluate(ticketError!);
                                jobRecord.Status = evaluation.jobRecordStatus;
                                jobRecord.ErrorMessage = evaluation.message;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(TicketStatus), ticket.Status, null);
                        }

                        await _buildingGrbContext.SaveChangesAsync(innerCt);
                    });

                pendingJobRecords = pendingJobRecords.Where(x => x.Status == JobRecordStatus.Pending).ToList();

                if (pendingJobRecords.Any())
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
            }
        }
    }
}
