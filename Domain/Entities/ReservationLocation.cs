using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Demande de location portant sur une CATEGORIE et une periode.
/// Ne reference volontairement aucun vehicule : au moment de la reservation,
/// seule la categorie est connue (principe de modelisation central).
/// </summary>
public class ReservationLocation : BaseEntity
{
    public int ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public int CategorieVehiculeId { get; set; }

    public CategorieVehicule Categorie { get; set; } = null!;

    /// <summary>Debut de la periode demandee. DateTime : la partie horaire sert au calcul R4.</summary>
    public DateTime Debut { get; set; }

    /// <summary>Fin de la periode demandee. Doit etre strictement posterieure a Debut (regle R1).</summary>
    public DateTime Fin { get; set; }

    public StatutReservationLocation Statut { get; set; } = StatutReservationLocation.EnAttente;

    /// <summary>
    /// Montant previsionnel calcule cote serveur a la creation (regle R4.1).
    /// N'est jamais recu du client.
    /// </summary>
    public decimal MontantEstime { get; set; }

    /// <summary>
    /// Contrat issu de cette reservation. Null tant que la location n'a pas demarre.
    /// Cote 0..1 de la relation transaction / detail (relation E).
    /// </summary>
    public ContratLocation? Contrat { get; set; }
}
