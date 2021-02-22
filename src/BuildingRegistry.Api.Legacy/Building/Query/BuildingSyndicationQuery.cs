namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.BuildingSyndication;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class BuildingSyndicationQuery : Query<BuildingSyndicationItem, BuildingSyndicationFilter, BuildingSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embedEvent;
        private readonly bool _embedObject;

        public BuildingSyndicationQuery(LegacyContext context, EmbedValue embed)
        {
            _context = context;
            _embedEvent = embed?.Event ?? false;
            _embedObject = embed?.Object ?? false;
        }

        protected override ISorting Sorting => new BuildingSyndicationSorting();

        protected override Expression<Func<BuildingSyndicationItem, BuildingSyndicationQueryResult>> Transformation
        {
            get
            {
                if (_embedEvent && _embedObject)
                    return x => BuildingSyndicationQueryResult(x, true);

                if (_embedEvent)
                    return x => new BuildingSyndicationQueryResult(
                        x.BuildingId.Value,
                        x.Position,
                        x.PersistentLocalId,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.EventDataAsXml);

                if (_embedObject)
                    return x => BuildingSyndicationQueryResult(x, false);

                return x => new BuildingSyndicationQueryResult(
                    x.BuildingId.Value,
                    x.Position,
                    x.PersistentLocalId,
                    x.ChangeType,
                    x.RecordCreatedAt,
                    x.LastChangedOn);
            }
        }

        private static BuildingSyndicationQueryResult BuildingSyndicationQueryResult(BuildingSyndicationItem x, bool withXml)
        {
            if (withXml)
                return new BuildingSyndicationQueryResult(
                    x.BuildingId.Value,
                    x.Position,
                    x.PersistentLocalId,
                    x.Status,
                    x.GeometryMethod,
                    x.Geometry,
                    x.ChangeType,
                    x.RecordCreatedAt,
                    x.LastChangedOn,
                    x.IsComplete,
                    x.Organisation,
                    x.Reason,
                    x.BuildingUnits,
                    x.EventDataAsXml);

            return new BuildingSyndicationQueryResult(
                x.BuildingId.Value,
                x.Position,
                x.PersistentLocalId,
                x.Status,
                x.GeometryMethod,
                x.Geometry,
                x.ChangeType,
                x.RecordCreatedAt,
                x.LastChangedOn,
                x.IsComplete,
                x.Organisation,
                x.Reason,
                x.BuildingUnits);
        }

        protected override IQueryable<BuildingSyndicationItem> Filter(FilteringHeader<BuildingSyndicationFilter> filtering)
        {
            var buildings = _context.BuildingSyndication.AsQueryable();

            if (_embedObject)
                buildings = buildings
                    .Include(x => x.BuildingUnits).ThenInclude(x => x.Addresses)
                    .Include(x => x.BuildingUnits).ThenInclude(x => x.Readdresses);

            buildings = buildings
                .OrderBy(x => x.Position)
                .AsNoTracking();

            if (!filtering.ShouldFilter || !filtering.Filter.Position.HasValue)
                return buildings;

            buildings = buildings.Where(m => m.Position >= filtering.Filter.Position);

            return buildings;
        }
    }

    public class BuildingSyndicationSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingSyndicationItem.Position), SortOrder.Ascending);
    }

    public class BuildingSyndicationFilter
    {
        public long? Position { get; set; }
        public EmbedValue Embed { get; set; }
    }
}
