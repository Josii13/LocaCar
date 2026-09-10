namespace Application.Locations.Dtos;

/// <summary>
/// Corps de POST /api/contrats/{id}/retour.
/// RetourReel est facultatif : absent, le service prend l'instant courant.
/// </summary>
public class EnregistrerRetourDto
{
    public DateTime? RetourReel { get; set; }
}
