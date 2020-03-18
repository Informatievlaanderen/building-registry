namespace BuildingRegistry.Importer.Console.Crab
{
    using Aiv.Vbr.CrabModel;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.ValueObjects.Crab;
    using System;

    public static class CrabMappings
    {
        public static CrabBuildingStatus ParseGebouwStatus(CrabGebouwstatus gebouwStatus)
        {
            //if (gebouwStatus == null)
            //    return null;

            if (gebouwStatus.Code == CrabGebouwstatus.VergunningAangevraagd.Code)
                return CrabBuildingStatus.PermitRequested;

            if (gebouwStatus.Code == CrabGebouwstatus.BouwvergunningVerleend.Code)
                return CrabBuildingStatus.BuildingPermitGranted;

            if (gebouwStatus.Code == CrabGebouwstatus.InAanbouw.Code)
                return CrabBuildingStatus.UnderConstruction;

            if (gebouwStatus.Code == CrabGebouwstatus.InGebruik.Code)
                return CrabBuildingStatus.InUse;

            if (gebouwStatus.Code == CrabGebouwstatus.BuitenGebruik.Code)
                return CrabBuildingStatus.OutOfUse;

            throw new Exception($"Onbekende gebouw status {gebouwStatus.Code}");
        }

        public static CrabBuildingGeometryMethod ParseGebouwGeometrieMethode(CrabGebouwgeometrieMethodeEnum gebouwGeometrieMethode)
        {
            if (gebouwGeometrieMethode.Code == CrabGebouwgeometrieMethodeEnum.Geschetst.Code)
                return CrabBuildingGeometryMethod.Outlined;

            if (gebouwGeometrieMethode.Code == CrabGebouwgeometrieMethodeEnum.Opmeting.Code)
                return CrabBuildingGeometryMethod.Survey;

            if (gebouwGeometrieMethode.Code == CrabGebouwgeometrieMethodeEnum.Grb.Code)
                return CrabBuildingGeometryMethod.Grb;

            throw new Exception($"Onbekende gebouw geometrie methode {gebouwGeometrieMethode.Code}");
        }

        public static CrabModification? ParseBewerking(CrabBewerking bewerking)
        {
            if (bewerking == null)
                return null;

            if (bewerking.Code == CrabBewerking.Invoer.Code)
                return CrabModification.Insert;

            if (bewerking.Code == CrabBewerking.Correctie.Code)
                return CrabModification.Correction;

            if (bewerking.Code == CrabBewerking.Historering.Code)
                return CrabModification.Historize;

            if (bewerking.Code == CrabBewerking.Verwijdering.Code)
                return CrabModification.Delete;

            throw new Exception($"Onbekende bewerking {bewerking.Code}");
        }

        public static CrabOrganisation? ParseOrganisatie(CrabOrganisatieEnum organisatie)
        {
            if (organisatie == null)
                return null;

            if (CrabOrganisatieEnum.AKRED.Code == organisatie.Code)
                return CrabOrganisation.Akred;

            if (CrabOrganisatieEnum.Andere.Code == organisatie.Code)
                return CrabOrganisation.Other;

            if (CrabOrganisatieEnum.DePost.Code == organisatie.Code)
                return CrabOrganisation.DePost;

            if (CrabOrganisatieEnum.Gemeente.Code == organisatie.Code)
                return CrabOrganisation.Municipality;

            if (CrabOrganisatieEnum.NGI.Code == organisatie.Code)
                return CrabOrganisation.Ngi;

            if (CrabOrganisatieEnum.NavTeq.Code == organisatie.Code)
                return CrabOrganisation.NavTeq;

            if (CrabOrganisatieEnum.Rijksregister.Code == organisatie.Code)
                return CrabOrganisation.NationalRegister;

            if (CrabOrganisatieEnum.TeleAtlas.Code == organisatie.Code)
                return CrabOrganisation.TeleAtlas;

            if (CrabOrganisatieEnum.VKBO.Code == organisatie.Code)
                return CrabOrganisation.Vkbo;

            if (CrabOrganisatieEnum.VLM.Code == organisatie.Code)
                return CrabOrganisation.Vlm;

            throw new Exception($"Onbekende organisatie {organisatie.Code}");
        }

        public static CrabAddressPositionOrigin ParseHerkomstAdresPositie(CrabHerkomstAdrespositieEnum herkomstAdrespositie)
        {
            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanLot.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromLot;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanPerceel.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromParcel;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanGebouw.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromBuilding;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanBrievenbus.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromMailbox;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanNutsaansluiting.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanToegangTotDeWeg.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanIngangVanGebouw.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanStandplaats.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromStand;

            if (CrabHerkomstAdrespositieEnum.ManueleAanduidingVanLigplaats.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.ManualIndicationFromBerth;

            if (CrabHerkomstAdrespositieEnum.AfgeleidVanGebouw.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.DerivedFromBuilding;

            if (CrabHerkomstAdrespositieEnum.AfgeleidVanPerceelGrb.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.DerivedFromParcelGrb;

            if (CrabHerkomstAdrespositieEnum.AfgeleidVanPerceelKadaster.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.DerivedFromParcelCadastre;

            if (CrabHerkomstAdrespositieEnum.GeinterpoleerdObvNevenliggendeHuisnummersGebouw.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding;

            if (CrabHerkomstAdrespositieEnum.GeinterpoleerdObvNevenliggendeHuisnummersPerceelGrb.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb;

            if (CrabHerkomstAdrespositieEnum.GeinterpoleerdObvNevenliggendeHuisnummersPerceelKadaster.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre;

            if (CrabHerkomstAdrespositieEnum.GeinterpoleerdObvWegverbinding.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection;

            if (CrabHerkomstAdrespositieEnum.AfgeleidVanStraat.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.DerivedFromStreet;

            if (CrabHerkomstAdrespositieEnum.AfgeleidVanGemeente.Code == herkomstAdrespositie.Code)
                return CrabAddressPositionOrigin.DerivedFromMunicipality;

            throw new Exception($"Onbekende herkomst adrespositie {herkomstAdrespositie.Code}");
        }
    }
}
