namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using GrArWKBReaderFactory = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory;

    public class MunicipalityGeometryRepository : IMunicipalityGeometryRepository
    {
        private static readonly Instant CutoffDate = Instant.FromUtc(2025, 1, 1, 0, 0);

        private readonly string _connectionString;
        private readonly object _lock = new();

        private List<CachedMunicipalityGeometry>? _cachedGeometries;
        private List<CachedMunicipalityGeometry>? _cachedGeometries2019;

        public MunicipalityGeometryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex, Instant eventTimestamp)
        {
            EnsureCacheLoaded();

            var ewkbBytes = extendedWkbGeometryAsHex.ToByteArray();
            var wkbReader = GrArWKBReaderFactory.CreateForLambert72();
            var buildingGeometry = wkbReader.Read(ewkbBytes);
            var srid = buildingGeometry.SRID;

            var geometries = eventTimestamp >= CutoffDate
                ? _cachedGeometries!
                : _cachedGeometries2019!;

            return geometries
                .Where(m => m.Srid == srid && m.Geometry.Intersects(buildingGeometry))
                .Select(m => m.NisCode)
                .Distinct()
                .ToList();
        }

        private void EnsureCacheLoaded()
        {
            if (_cachedGeometries is not null)
                return;

            lock (_lock)
            {
                if (_cachedGeometries is not null)
                    return;

                _cachedGeometries = LoadGeometries("integration_municipality.municipality_geometries");
                _cachedGeometries2019 = LoadGeometries("integration_municipality.municipality_geometries_2019");
            }
        }

        private List<CachedMunicipalityGeometry> LoadGeometries(string tableName)
        {
            var results = new List<CachedMunicipalityGeometry>();

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT nis_code, geometry FROM {tableName}";
            command.CommandTimeout = 60 * 10;

            var wkbReader = GrArWKBReaderFactory.CreateForLambert72();

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var nisCode = reader.GetString(0);
                var geometryBytes = (byte[])reader[1];
                var geometry = wkbReader.Read(geometryBytes);

                results.Add(new CachedMunicipalityGeometry(nisCode, geometry.SRID, geometry));
            }

            return results;
        }

        private sealed record CachedMunicipalityGeometry(string NisCode, int Srid, Geometry Geometry);
    }
}
