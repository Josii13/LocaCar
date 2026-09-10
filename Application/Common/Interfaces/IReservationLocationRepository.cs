using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

/// <summary>Requetes propres aux reservations.</summary>
public interface IReservationLocationRepository : IRepository<ReservationLocation>
{
    /// <summary>
    /// Reservations de la categorie qui occupent de la capacite sur la periode :
    /// statut dans <paramref name="statutsActifs"/> et periode chevauchante (R2).
    /// </summary>
    IQueryable<ReservationLocation> ActivesChevauchantes(
        int categorieVehiculeId, DateTime debut, DateTime fin,
        IReadOnlyCollection<StatutReservationLocation> statutsActifs);

    /// <summary>Lecture seule avec client, categorie et contrat eventuel.</summary>
    Task<ReservationLocation?> ObtenirAvecDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Entite suivie, avec sa categorie : ce qu'il faut pour demarrer la location (R3).</summary>
    Task<ReservationLocation?> ObtenirAvecCategorieAsync(int id, CancellationToken ct = default);
}
