using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>Requetes propres aux contrats.</summary>
public interface IContratLocationRepository : IRepository<ContratLocation>
{
    /// <summary>Lecture seule avec le vehicule affecte.</summary>
    Task<ContratLocation?> ObtenirAvecVehiculeAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Entite suivie, avec le vehicule et la categorie de la reservation : ce qu'il faut
    /// pour enregistrer le retour et calculer le montant final (R4).
    /// </summary>
    Task<ContratLocation?> ObtenirPourRetourAsync(int id, CancellationToken ct = default);
}
