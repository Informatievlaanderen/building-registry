namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;

    public interface IJobRecordsArchiver
    {
        Task Archive(Guid jobId, CancellationToken ct);
    }

    public class JobRecordsArchiver : IJobRecordsArchiver
    {
        private readonly string _connectionString;
        private readonly ILogger<JobRecordsArchiver> _logger;

        public JobRecordsArchiver(string connectionString, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _logger = loggerFactory.CreateLogger<JobRecordsArchiver>();
        }

        public async Task Archive(Guid jobId, CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var transaction = connection.BeginTransaction();

                try
                {
                    await ArchiveRecords(connection, transaction, jobId);
                    await RemoveRecords(connection, transaction, jobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Rolling back archiving of jobRecords for job '{jobId}' because of Exception:", ex);
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
        }

        private static async Task RemoveRecords(SqlConnection connection,
            SqlTransaction transaction,
            Guid jobId)
        {
            await connection.ExecuteAsync(
                $"DELETE FROM [{BuildingGrbContext.Schema}].[{JobRecordConfiguration.TableName}] WHERE [JobId] = @jobId;",
                new { jobId },
                transaction);
        }

        private static async Task ArchiveRecords(
            SqlConnection connection,
            SqlTransaction transaction,
            Guid jobId)
        {
            await connection.ExecuteAsync($@"
INSERT INTO [{BuildingGrbContext.Schema}].[{JobRecordConfiguration.ArchiveTableName}]
    ([Id]
    ,[JobId]
    ,[Idn]
    ,[IdnVersion]
    ,[VersionDate]
    ,[EndDate]
    ,[GrbObject]
    ,[GrbObjectType]
    ,[EventType]
    ,[GrId]
    ,[Geometry]
    ,[Overlap]
    ,[Status]
    ,[ErrorMessage]
    ,[BuildingPersistentLocalId]
    ,[TicketId])
SELECT [Id]
    ,[JobId]
    ,[Idn]
    ,[IdnVersion]
    ,[VersionDate]
    ,[EndDate]
    ,[GrbObject]
    ,[GrbObjectType]
    ,[EventType]
    ,[GrId]
    ,[Geometry]
    ,[Overlap]
    ,[Status]
    ,[ErrorMessage]
    ,[BuildingPersistentLocalId]
    ,[TicketId]
FROM [{BuildingGrbContext.Schema}].[{JobRecordConfiguration.TableName}]
WHERE [JobId] = @jobId", new { jobId }, transaction);
        }
    }
}
