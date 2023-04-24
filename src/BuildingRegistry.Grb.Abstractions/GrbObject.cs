namespace BuildingRegistry.Grb.Abstractions
{
    public enum GrbObject
    {
        Unknown = 0,
        BuildingAtGroundLevel = 1, // gbg, gebouw aan de grond
        CompositeBuilding = 2, // sbg, samengesteld gebouw
        ArtWork = 3 // knw, kunstwerk
    }
}
