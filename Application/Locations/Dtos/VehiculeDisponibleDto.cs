namespace Application.Locations.Dtos;

/// <summary>Ligne de resultat de GET /api/vehicules/disponibles.</summary>
public class VehiculeDisponibleDto
{
    public int Id { get; set; }

    public string Immatriculation { get; set; } = string.Empty;

    public string Modele { get; set; } = string.Empty;

    public string CategorieCode { get; set; } = string.Empty;

    public string AgenceCode { get; set; } = string.Empty;

    public string AgenceVille { get; set; } = string.Empty;
}
