namespace Domain.Enums;

/// <summary>
/// Etat d'un vehicule du parc.
/// Seul un vehicule Disponible peut etre affecte a une location (regle R3).
/// </summary>
public enum StatutVehicule
{
    Disponible = 0,
    Loue = 1,
    Maintenance = 2,
    HorsService = 3
}
