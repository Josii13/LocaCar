using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ContratLocationRepository : Repository<ContratLocation>, IContratLocationRepository
{
    public ContratLocationRepository(LocaCarDbContext contexte) : base(contexte)
    {
    }

    public Task<ContratLocation?> ObtenirAvecVehiculeAsync(int id, CancellationToken ct = default) =>
        Ensemble
            .Include(c => c.Vehicule)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<ContratLocation?> ObtenirPourRetourAsync(int id, CancellationToken ct = default) =>
        Ensemble
            .Include(c => c.Vehicule)
            .Include(c => c.Reservation)
                .ThenInclude(r => r.Categorie)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}
