using System.ComponentModel.DataAnnotations;

namespace Application.Locations.Dtos;

/// <summary>
/// Corps de POST /api/reservations-locations.
/// Ne porte que de la validation de forme : "la requete est-elle bien remplie ?".
/// Les regles R1 et R2 sont verifiees par le service, pas ici.
/// MontantEstime est volontairement absent : il est calcule cote serveur.
/// </summary>
public class CreerReservationDto
{
    [Required(ErrorMessage = "Le client est obligatoire.")]
    [Range(1, int.MaxValue, ErrorMessage = "L'identifiant du client est invalide.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "La categorie de vehicule est obligatoire.")]
    [Range(1, int.MaxValue, ErrorMessage = "L'identifiant de la categorie est invalide.")]
    public int CategorieVehiculeId { get; set; }

    [Required(ErrorMessage = "La date de debut est obligatoire.")]
    public DateTime Debut { get; set; }

    [Required(ErrorMessage = "La date de fin est obligatoire.")]
    public DateTime Fin { get; set; }
}
