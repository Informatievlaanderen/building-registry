namespace BuildingRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Dapper;
    using Npgsql;

    public interface IAddresses
    {
        Task<int?> GetAddressPersistentLocalId(Guid addressId);
        public Task AddAddressPersistentLocalId(Guid addressId, int persistentLocalId);
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

            var sql = @"SELECT persistent_local_id
	                    FROM integration_address.address_id_address_persistent_local_id
	                    WHERE address_id = @AddressId;";

            return await connection.QuerySingleOrDefaultAsync<int?>(sql, new
            {
                AddressId = addressId
            });
        }

        public async Task AddAddressPersistentLocalId(Guid addressId, int persistentLocalId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sql = @"INSERT INTO integration_address.address_id_address_persistent_local_id (address_id, persistent_local_id)
                        VALUES (@AddressId, @PersistentLocalId);";

            await connection.ExecuteAsync(sql, new
            {
                AddressId = addressId,
                PersistentLocalId = persistentLocalId
            });
        }
    }
}
