namespace Application.Locations.Dtos;

/// <summary>Representation exposee d'une reservation. Le statut est rendu en texte.</summary>
public class ReservationDto
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string ClientNom { get; set; } = string.Empty;

    public int CategorieVehiculeId { get; set; }

    public string CategorieCode { get; set; } = string.Empty;

    public DateTime Debut { get; set; }

    public DateTime Fin { get; set; }

    public string Statut { get; set; } = string.Empty;

    public decimal MontantEstime { get; set; }

    /// <summary>Identifiant du contrat issu de la reservation. Null tant qu'elle n'a pas demarre.</summary>
    public int? ContratId { get; set; }
}
