using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Vehicule physique du parc. Seul un vehicule de statut Disponible peut etre
/// affecte a une location (regle R3).
/// </summary>
public class Vehicule : BaseEntity
{
    /// <summary>Plaque du vehicule. Unique parmi les vehicules non supprimes.</summary>
    [Required, MaxLength(20)]
    public string Immatriculation { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Modele { get; set; } = string.Empty;

    public StatutVehicule Statut { get; set; } = StatutVehicule.Disponible;

    public int CategorieVehiculeId { get; set; }

    public CategorieVehicule Categorie { get; set; } = null!;

    public int AgenceId { get; set; }

    public Agence Agence { get; set; } = null!;

    /// <summary>
    /// Historique des contrats de ce vehicule. Sert au test de chevauchement
    /// de la recherche de disponibilite.
    /// </summary>
    public ICollection<ContratLocation> Contrats { get; set; } = new List<ContratLocation>();
}
