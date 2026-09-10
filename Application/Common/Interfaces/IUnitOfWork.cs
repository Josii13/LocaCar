using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Unite de travail : regroupe les repositories qui partagent le meme DbContext
/// pendant une requete, et porte l'unique point d'enregistrement.
/// Un cas d'utilisation lit et modifie a travers les repositories, puis appelle
/// SaveChangesAsync une seule fois : EF Core envoie tout dans une meme transaction.
/// </summary>
public interface IUnitOfWork
{
    IRepository<Agence> Agences { get; }

    IRepository<Client> Clients { get; }

    ICategorieVehiculeRepository CategoriesVehicules { get; }

    IVehiculeRepository Vehicules { get; }

    IReservationLocationRepository ReservationsLocations { get; }

    IContratLocationRepository ContratsLocations { get; }

    /// <summary>
    /// Ecrit toutes les modifications suivies en une seule transaction.
    /// C'est ce point unique qui rend R3 et R4 atomiques.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
