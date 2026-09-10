using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>Requetes propres au parc de vehicules.</summary>
public interface IVehiculeRepository : IRepository<Vehicule>
{
    /// <summary>
    /// Vehicules d'une categorie affectables sur la periode : statut Disponible et aucun
    /// contrat chevauchant. Definition unique de "vehicule libre", partagee par UC1, par le
    /// comptage de R2 et par la selection de R3.
    /// Un contrat immobilise le vehicule depuis Depart jusqu'au retour reel s'il a eu lieu,
    /// jusqu'au retour prevu sinon. Bornes exclusives : une location finissant a 08h00 ne
    /// chevauche pas une location commencant a 08h00.
    /// </summary>
    IQueryable<Vehicule> Libres(int categorieVehiculeId, DateTime debut, DateTime fin);
}
