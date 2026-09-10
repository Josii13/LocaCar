using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Referentiel tarifaire. C'est une categorie que l'on reserve, pas un vehicule :
/// le vehicule physique n'est affecte qu'au demarrage de la location (regle R3).
/// Relations B et D du modele.
/// </summary>
public class CategorieVehicule : BaseEntity
{
    /// <summary>Identifiant metier de la categorie. Unique parmi les categories non supprimees.</summary>
    public string Code { get; set; } = string.Empty;

    public string Libelle { get; set; } = string.Empty;

    /// <summary>Prix d'une journee entamee. Base des calculs de la regle R4.</summary>
    public decimal TarifJournalier { get; set; }

    /// <summary>
    /// Montant de la caution. Decision D4 : stockee et affichee uniquement,
    /// elle n'entre dans aucun calcul de montant.
    /// </summary>
    public decimal Caution { get; set; }

    /// <summary>Penalite appliquee par jour de retard entame (regle R4).</summary>
    public decimal PenaliteRetardParJour { get; set; }

    /// <summary>Vehicules appartenant a cette categorie.</summary>
    public ICollection<Vehicule> Vehicules { get; set; } = new List<Vehicule>();

    /// <summary>Reservations portant sur cette categorie.</summary>
    public ICollection<ReservationLocation> Reservations { get; set; } = new List<ReservationLocation>();
}
