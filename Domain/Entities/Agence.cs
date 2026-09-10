using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Agence physique du reseau. Point de rattachement d'un vehicule.
/// Relation A du modele : Agence 1 - * Vehicule.
/// </summary>
public class Agence : BaseEntity
{
    /// <summary>Identifiant metier de l'agence. Unique parmi les agences non supprimees.</summary>
    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string Ville { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Adresse { get; set; } = string.Empty;

    /// <summary>Vehicules rattaches a cette agence.</summary>
    public ICollection<Vehicule> Vehicules { get; set; } = new List<Vehicule>();
}
