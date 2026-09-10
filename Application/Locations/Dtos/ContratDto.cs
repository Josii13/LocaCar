namespace Application.Locations.Dtos;

/// <summary>Representation exposee d'un contrat de location.</summary>
public class ContratDto
{
    public int Id { get; set; }

    public int ReservationLocationId { get; set; }

    public int VehiculeId { get; set; }

    public string VehiculeImmatriculation { get; set; } = string.Empty;

    public DateTime Depart { get; set; }

    public DateTime RetourPrevu { get; set; }

    /// <summary>Null tant que le vehicule n'est pas rendu : sert aussi d'indicateur d'etat.</summary>
    public DateTime? RetourReel { get; set; }

    /// <summary>Null tant que le retour n'a pas eu lieu (regle R4).</summary>
    public decimal? MontantFinal { get; set; }

    /// <summary>Detail du calcul R4, renseigne uniquement apres le retour.</summary>
    public int? JoursFactures { get; set; }

    public int? JoursRetard { get; set; }
}
