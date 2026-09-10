using Application.Locations.Dtos;

namespace Application.Locations;

/// <summary>
/// Cas d'utilisation de la location. C'est le seul endroit ou vivent les regles
/// R1 a R4 : ni les controleurs, ni les vues Razor ne les reimplementent.
/// </summary>
public interface ILocationService
{
    /// <summary>UC1 — vehicules affectables sur la periode pour une categorie.</summary>
    Task<IReadOnlyList<VehiculeDisponibleDto>> ObtenirVehiculesDisponiblesAsync(
        int categorieVehiculeId, DateTime debut, DateTime fin, CancellationToken ct = default);

    /// <summary>Lecture d'une reservation. Cible de l'en-tete Location renvoye par UC2.</summary>
    Task<ReservationDto> ObtenirReservationAsync(int id, CancellationToken ct = default);

    /// <summary>Lecture d'un contrat. Cible de l'en-tete Location renvoye par UC3.</summary>
    Task<ContratDto> ObtenirContratAsync(int id, CancellationToken ct = default);

    /// <summary>UC2 — cree une reservation apres application de R1 puis R2.</summary>
    Task<ReservationDto> CreerReservationAsync(CreerReservationDto demande, CancellationToken ct = default);

    /// <summary>UC3 — demarre la location (R3), en une seule unite de travail.</summary>
    Task<ContratDto> DemarrerLocationAsync(int reservationId, CancellationToken ct = default);

    /// <summary>UC4 — enregistre le retour et calcule le montant final (R4).</summary>
    Task<ContratDto> EnregistrerRetourAsync(int contratId, DateTime? retourReel, CancellationToken ct = default);
}
