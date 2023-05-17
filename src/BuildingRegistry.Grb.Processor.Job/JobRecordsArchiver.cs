namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using Abstractions;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;

    public interface IJobRecordsArchiver
    {
        void Archive(Guid jobId);
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

        public void Archive(Guid jobId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

                try
                {
                    ArchiveRecords(connection, jobId, transaction);
                    RemoveRecords(connection, jobId, transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Rolling back archiving of jobRecords for job '{jobId}' because of Exception:", ex);
                    transaction.Rollback();

                    throw;
                }

                transaction.Commit();
        }

        private static void RemoveRecords(SqlConnection connection, Guid jobId, SqlTransaction transaction)
        {
            connection.Execute(
                $"DELETE FROM [{JobRecordConfiguration.TableName}] WHERE [JobId] = @jobId;",
                new { jobId },
                transaction);
        }

        private static void ArchiveRecords(SqlConnection connection, Guid jobId, SqlTransaction transaction)
        {
            connection.Execute($@"
INSERT INTO [{JobRecordConfiguration.ArchiveTableName}] ([Id]
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
FROM [{JobRecordConfiguration.TableName}]
WHERE [JobId] = @jobId", new { jobId }, transaction);
        }
    }
}
