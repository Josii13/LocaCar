using System.ComponentModel.DataAnnotations;

namespace Application.Referentiel.Dtos;

/// <summary>
/// Corps de POST et PUT sur /api/categories-vehicules, et modele des formulaires MVC.
/// Ne porte que de la validation de forme : l'unicite du code est une regle d'etat,
/// verifiee par le service et rendue par un 409.
/// </summary>
public class EnregistrerCategorieVehiculeDto
{
    [Required(ErrorMessage = "Le code est obligatoire.")]
    [StringLength(10, ErrorMessage = "Le code ne peut pas depasser 10 caracteres.")]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le libelle est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le libelle ne peut pas depasser 100 caracteres.")]
    [Display(Name = "Libelle")]
    public string Libelle { get; set; } = string.Empty;

    [Range(0.01, 99999999.99, ErrorMessage = "Le tarif journalier doit etre strictement positif.")]
    [Display(Name = "Tarif journalier")]
    public decimal TarifJournalier { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "La caution ne peut pas etre negative.")]
    [Display(Name = "Caution")]
    public decimal Caution { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "La penalite de retard ne peut pas etre negative.")]
    [Display(Name = "Penalite de retard par jour")]
    public decimal PenaliteRetardParJour { get; set; }
}
