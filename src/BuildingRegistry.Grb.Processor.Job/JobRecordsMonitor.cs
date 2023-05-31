namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.EntityFrameworkCore;
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
            var pendingJobRecords = await _buildingGrbContext.JobRecords
                .Where(x => x.JobId == jobId && x.Status == JobRecordStatus.Pending)
                .OrderBy(x => x.Id)
                .ToListAsync(ct);

            while (pendingJobRecords.Any())
            {
                foreach (var jobRecord in pendingJobRecords)
                {
                    var ticket = await _ticketing.Get(jobRecord.TicketId!.Value, ct);

                    switch (ticket!.Status)
                    {
                        case TicketStatus.Created:
                        case TicketStatus.Pending:
                            break;
                        case TicketStatus.Complete:
                            var etagResponse =
                                JsonConvert.DeserializeObject<ETagResponse>(ticket.Result!.ResultAsJson!);
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

                    await _buildingGrbContext.SaveChangesAsync(ct);
                }

                pendingJobRecords = pendingJobRecords
                    .Where(x => x.Status == JobRecordStatus.Pending)
                    .OrderBy(x => x.Id)
                    .ToList();

                if (pendingJobRecords.Any())
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
            }
        }

        // Untested, parallel code
        // public async Task Monitor(Guid jobId, CancellationToken ct)
        // {
        //     int pendingJobRecordsCount;
        //     await using (var buildingGrbContext = await _buildingGrbContextFactory.CreateDbContextAsync(ct))
        //     {
        //         pendingJobRecordsCount = await buildingGrbContext.JobRecords
        //             .CountAsync(x => x.JobId == jobId && x.Status == JobRecordStatus.Pending, cancellationToken: ct);
        //     }
        //
        //     while (pendingJobRecordsCount > 0)
        //     {
        //         // var chunkSize = (int)Math.Ceiling(pendingJobRecords.Count / 10.0);
        //         const int chunkSize = 50;
        //
        //         await Parallel.ForEachAsync(
        //             Enumerable.Range(0, (int) Math.Ceiling(pendingJobRecordsCount / (decimal)chunkSize)),
        //             new ParallelOptions { MaxDegreeOfParallelism = 10 },
        //             async (index, innerCt) =>
        //             {
        //                 var buildingGrbContext = await _buildingGrbContextFactory.CreateDbContextAsync(ct);
        //                 var jobRecords = buildingGrbContext.JobRecords
        //                     .Where(x => x.JobId == jobId && x.Status == JobRecordStatus.Pending)
        //                     .OrderBy(x => x.Id)
        //                     .Skip(index * chunkSize)
        //                     .Take(chunkSize);
        //
        //                 foreach (var jobRecord in jobRecords)
        //                 {
        //                     var ticket = await _ticketing.Get(jobRecord.TicketId!.Value, innerCt);
        //
        //                     switch (ticket!.Status)
        //                     {
        //                         case TicketStatus.Created:
        //                         case TicketStatus.Pending:
        //                             break;
        //                         case TicketStatus.Complete:
        //                             var etagResponse = JsonConvert.DeserializeObject<ETagResponse>(ticket.Result!.ResultAsJson!);
        //                             jobRecord.BuildingPersistentLocalId = etagResponse!.Location.AsIdentifier().Map(int.Parse);
        //                             jobRecord.Status = JobRecordStatus.Complete;
        //                             break;
        //                         case TicketStatus.Error:
        //                             var ticketError = JsonConvert.DeserializeObject<TicketError>(ticket.Result!.ResultAsJson!);
        //                             var evaluation = ErrorWarningEvaluator.Evaluate(ticketError!);
        //                             jobRecord.Status = evaluation.jobRecordStatus;
        //                             jobRecord.ErrorMessage = evaluation.message;
        //                             break;
        //                         default:
        //                             throw new ArgumentOutOfRangeException(nameof(TicketStatus), ticket.Status, null);
        //                     }
        //
        //                     buildingGrbContext.SaveChangesAsync(ct);
        //                 }
        //             });
        //
        //
        //         await using var buildingGrbContext = await _buildingGrbContextFactory.CreateDbContextAsync(ct);
        //         pendingJobRecordsCount = await buildingGrbContext.JobRecords
        //              .CountAsync(x => x.JobId == jobId && x.Status == JobRecordStatus.Pending, cancellationToken: ct);
        //
        //         if (pendingJobRecordsCount > 0)
        //         {
        //             await Task.Delay(TimeSpan.FromSeconds(5), ct);
        //         }
        //     }
        // }
    }
}
