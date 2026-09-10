using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Materialisation transactionnelle d'une reservation : c'est ici que le vehicule
/// physique est engage. Cree par la regle R3, clos par la regle R4.
/// Detail de la relation transaction / detail (relation E), au plus un par reservation.
/// </summary>
public class ContratLocation : BaseEntity
{
    public int ReservationLocationId { get; set; }

    public ReservationLocation Reservation { get; set; } = null!;

    public int VehiculeId { get; set; }

    public Vehicule Vehicule { get; set; } = null!;

    /// <summary>Instant de sortie effective du vehicule.</summary>
    public DateTime Depart { get; set; }

    /// <summary>Instant de retour convenu. Sert de reference au calcul du retard (regle R4).</summary>
    public DateTime RetourPrevu { get; set; }

    /// <summary>
    /// Instant de retour reel. Null tant que le vehicule n'est pas rendu :
    /// l'information n'existe pas encore, elle ne vaut pas zero. Sert aussi
    /// d'indicateur d'etat du contrat (null = en cours, renseigne = clos).
    /// </summary>
    public DateTime? RetourReel { get; set; }

    /// <summary>
    /// Montant facture au retour (regle R4.2). Null tant que le retour n'a pas eu lieu :
    /// un zero signifierait "rendu, gratuitement", ce qui serait faux.
    /// </summary>
    public decimal? MontantFinal { get; set; }
}
