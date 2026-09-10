using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ReservationLocationRepository : Repository<ReservationLocation>, IReservationLocationRepository
{
    public ReservationLocationRepository(LocaCarDbContext contexte) : base(contexte)
    {
    }

    /// <inheritdoc/>
    public IQueryable<ReservationLocation> ActivesChevauchantes(
        int categorieVehiculeId, DateTime debut, DateTime fin,
        IReadOnlyCollection<StatutReservationLocation> statutsActifs) =>
        Ensemble.Where(r => r.CategorieVehiculeId == categorieVehiculeId
                            && statutsActifs.Contains(r.Statut)
                            && r.Debut < fin
                            && debut < r.Fin);

    public Task<ReservationLocation?> ObtenirAvecDetailsAsync(int id, CancellationToken ct = default) =>
        Ensemble
            .Include(r => r.Client)
            .Include(r => r.Categorie)
            .Include(r => r.Contrat)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<ReservationLocation?> ObtenirAvecCategorieAsync(int id, CancellationToken ct = default) =>
        Ensemble
            .Include(r => r.Categorie)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
}
