namespace Application.Referentiel.Dtos;

/// <summary>
/// Representation exposee d'une categorie. Les entites EF Core ne sont jamais
/// serialisees : elles porteraient leurs navigations et leurs colonnes d'audit.
/// </summary>
public class CategorieVehiculeDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Libelle { get; set; } = string.Empty;

    public decimal TarifJournalier { get; set; }

    public decimal Caution { get; set; }

    public decimal PenaliteRetardParJour { get; set; }

    /// <summary>Nombre de vehicules rattaches. Sert a expliquer un refus de suppression.</summary>
    public int NombreVehicules { get; set; }
}
