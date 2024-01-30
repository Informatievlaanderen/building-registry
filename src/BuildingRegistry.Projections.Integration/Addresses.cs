namespace BuildingRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Dapper;
    using Npgsql;

    public interface IAddresses
    {
        Task<int?> GetAddressPersistentLocalId(Guid addressId);
    }

    public class Addresses : IAddresses
    {
        private readonly string _connectionString;

        public Addresses(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int?> GetAddressPersistentLocalId(Guid addressId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sql = @$"SELECT address_id, persistent_local_id
	                    FROM integration_address.address_id_address_persistent_local_id
	                    WHERE address_id = '{addressId}'";

            return await connection.QuerySingleOrDefaultAsync<int?>(sql);
        }
    }
}
