namespace BuildingRegistry.Projections.Syndication.Parcel
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "Perceel", Namespace = "")]
    public class Parcel
    {
        /// <summary>
        /// De technische id van het perceel.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid ParcelId { get; set; }

        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De status van het perceel.
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 3)]
        public PerceelStatus? Status { get; set; }

        /// <summary>
        /// De aan het perceel gelinkte adressen
        /// </summary>
        [DataMember(Name = "AdressenIds", Order = 4)]
        public List<Guid> AddressIds { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 5)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 6)]
        public Provenance Provenance { get; set; }

        public Parcel()
        {
            AddressIds = new List<Guid>();
        }
    }
}
