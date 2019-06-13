namespace BuildingRegistry.Projections.Syndication.Parcel
{
    public enum ParcelEvent
    {
        ParcelWasRegistered,
        ParcelWasRemoved,

        ParcelWasRetired,
        ParcelWasCorrectedToRetired,
        ParcelWasRealized,
        ParcelWasCorrectedToRealized,

        ParcelAddressWasAttached,
        ParcelAddressWasDetached
    }
}
