﻿namespace BuildingRegistry.Projections.Integration.Infrastructure
{
    public class IntegrationOptions
    {
        public string BuildingNamespace { get; set; }
        public string BuildingUnitNamespace { get; set; }
        public string EventsConnectionString { get; set; }
    }
}
