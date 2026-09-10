using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation de l'unite de travail. Le DbContext et chaque repository sont
/// enregistres en Scoped : pendant une requete HTTP, il n'existe qu'un seul
/// LocaCarDbContext, et tous les repositories injectes ici le partagent.
/// SaveChangesAsync delegue donc a ce contexte unique : une transaction pour tout.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LocaCarDbContext _contexte;

    public UnitOfWork(
        LocaCarDbContext contexte,
        IRepository<Agence> agences,
        IRepository<Client> clients,
        ICategorieVehiculeRepository categoriesVehicules,
        IVehiculeRepository vehicules,
        IReservationLocationRepository reservationsLocations,
        IContratLocationRepository contratsLocations)
    {
        _contexte = contexte;
        Agences = agences;
        Clients = clients;
        CategoriesVehicules = categoriesVehicules;
        Vehicules = vehicules;
        ReservationsLocations = reservationsLocations;
        ContratsLocations = contratsLocations;
    }

    public IRepository<Agence> Agences { get; }

    public IRepository<Client> Clients { get; }

    public ICategorieVehiculeRepository CategoriesVehicules { get; }

    public IVehiculeRepository Vehicules { get; }

    public IReservationLocationRepository ReservationsLocations { get; }

    public IContratLocationRepository ContratsLocations { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _contexte.SaveChangesAsync(ct);
}
