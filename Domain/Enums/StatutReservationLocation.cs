namespace Domain.Enums;

/// <summary>
/// Etat d'une reservation dans le workflow LocaCar.
/// EnAttente / Confirmee -> occupe de la capacite de la categorie (regle R2)
/// EnCours               -> un ContratLocation existe (regle R3)
/// Terminee              -> le montant final est calcule (regle R4)
/// </summary>
public enum StatutReservationLocation
{
    EnAttente = 0,
    Confirmee = 1,
    EnCours = 2,
    Terminee = 3,
    Annulee = 4
}
