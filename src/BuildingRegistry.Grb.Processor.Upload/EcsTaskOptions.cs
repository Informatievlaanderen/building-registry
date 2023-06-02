namespace BuildingRegistry.Grb.Processor.Upload
{
    public class EcsTaskOptions
    {
        public string TaskDefinition { get; set; }
        public string Cluster { get; set; }
        public string Subnets { get; set; }
        public string SecurityGroups { get; set; }
    }
}
