using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public class VehiculeRepository : Repository<Vehicule>, IVehiculeRepository
{
    public VehiculeRepository(LocaCarDbContext contexte) : base(contexte)
    {
    }

    /// <inheritdoc/>
    public IQueryable<Vehicule> Libres(int categorieVehiculeId, DateTime debut, DateTime fin) =>
        Ensemble.Where(v => v.CategorieVehiculeId == categorieVehiculeId
                            && v.Statut == StatutVehicule.Disponible
                            && !v.Contrats.Any(c => c.Depart < fin
                                                    && debut < (c.RetourReel ?? c.RetourPrevu)));
}
