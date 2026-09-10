using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Agence physique du reseau. Point de rattachement d'un vehicule.
/// Relation A du modele : Agence 1 - * Vehicule.
/// </summary>
public class Agence : BaseEntity
{
    /// <summary>Identifiant metier de l'agence. Unique parmi les agences non supprimees.</summary>
    public string Code { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public string Ville { get; set; } = string.Empty;

    public string Adresse { get; set; } = string.Empty;

    /// <summary>Vehicules rattaches a cette agence.</summary>
    public ICollection<Vehicule> Vehicules { get; set; } = new List<Vehicule>();
}
