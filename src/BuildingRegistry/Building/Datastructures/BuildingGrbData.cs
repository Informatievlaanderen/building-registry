namespace BuildingRegistry.Building.Datastructures
{
    using System.Collections.Generic;
    using NodaTime;

    public class BuildingGrbData
    {
        public long Idn { get; }
        public Instant VersionDate { get; }
        public Instant? EndDate { get; }
        public int IdnVersion { get; }
        public string GrbObject { get; }
        public string GrbObjectType { get; }
        public string EventType { get; }
        public string Geometry { get; }
        public decimal? Overlap { get; }

        public BuildingGrbData(
            long idn,
            Instant versionDate,
            Instant? endDate,
            int idnVersion,
            string grbObject,
            string grbObjectType,
            string eventType,
            string geometry,
            decimal? overlap)
        {
            Idn = idn;
            VersionDate = versionDate;
            EndDate = endDate;
            IdnVersion = idnVersion;
            GrbObject = grbObject;
            GrbObjectType = grbObjectType;
            EventType = eventType;
            Geometry = geometry;
            Overlap = overlap;
        }

        internal IEnumerable<object> IdentityFields()
        {
            yield return Idn;
            yield return VersionDate;
            yield return EndDate;
            yield return IdnVersion;
            yield return GrbObject;
            yield return GrbObjectType;
            yield return EventType;
            yield return Geometry;
            yield return Overlap;
        }
    }
}
