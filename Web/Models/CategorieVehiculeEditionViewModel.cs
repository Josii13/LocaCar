using Application.Referentiel.Dtos;

namespace Web.Models;

/// <summary>
/// Modele du formulaire de modification : les champs saisissables du DTO, plus
/// l'identifiant de la categorie editee, qui voyage en champ cache pour verifier
/// que le formulaire poste correspond bien a l'URL.
/// Herite du DTO pour reutiliser ses attributs de validation et ses libelles.
/// </summary>
public class CategorieVehiculeEditionViewModel : EnregistrerCategorieVehiculeDto
{
    public int Id { get; set; }

    public static CategorieVehiculeEditionViewModel DepuisDto(CategorieVehiculeDto dto) => new()
    {
        Id = dto.Id,
        Code = dto.Code,
        Libelle = dto.Libelle,
        TarifJournalier = dto.TarifJournalier,
        Caution = dto.Caution,
        PenaliteRetardParJour = dto.PenaliteRetardParJour
    };
}
