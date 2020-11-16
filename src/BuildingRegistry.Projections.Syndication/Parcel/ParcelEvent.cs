namespace BuildingRegistry.Projections.Syndication.Parcel
{
    public enum ParcelEvent
    {
        ParcelWasRegistered,
        ParcelWasRemoved,
        ParcelWasRecovered,

        ParcelWasRetired,
        ParcelWasCorrectedToRetired,
        ParcelWasRealized,
        ParcelWasCorrectedToRealized,

        ParcelAddressWasAttached,
        ParcelAddressWasDetached
    }
}
