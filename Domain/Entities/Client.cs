using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Client de la societe de location.
/// Relation C du modele : Client 1 - * ReservationLocation.
/// </summary>
public class Client : BaseEntity
{
    /// <summary>Identifiant metier du client. Unique parmi les clients non supprimes.</summary>
    public string Numero { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public string Telephone { get; set; } = string.Empty;

    public string NumeroPermis { get; set; } = string.Empty;

    /// <summary>
    /// Date d'expiration du permis. DateOnly et non DateTime : c'est une date
    /// administrative, pas un instant. Comparee a la fin de la reservation par la regle R1.
    /// </summary>
    public DateOnly ExpirationPermis { get; set; }

    /// <summary>Reservations passees par ce client.</summary>
    public ICollection<ReservationLocation> Reservations { get; set; } = new List<ReservationLocation>();
}
