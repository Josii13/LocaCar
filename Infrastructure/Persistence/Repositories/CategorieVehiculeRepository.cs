using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategorieVehiculeRepository : Repository<CategorieVehicule>, ICategorieVehiculeRepository
{
    public CategorieVehiculeRepository(LocaCarDbContext contexte) : base(contexte)
    {
    }

    public Task<bool> CodeDejaUtiliseAsync(string code, int? idAExclure, CancellationToken ct = default) =>
        Ensemble.AnyAsync(c => c.Code == code && (idAExclure == null || c.Id != idAExclure), ct);
}
