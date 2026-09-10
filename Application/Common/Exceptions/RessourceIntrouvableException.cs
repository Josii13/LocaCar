namespace Application.Common.Exceptions;

/// <summary>
/// L'identifiant fourni ne designe aucune ligne visible. Correspond au 404 Not Found.
/// Une ligne supprimee logiquement est invisible : elle produit donc un 404, pas un 410.
/// </summary>
public class RessourceIntrouvableException : Exception
{
    public RessourceIntrouvableException(string nomRessource, int id)
        : base($"{nomRessource} {id} introuvable.")
    {
        NomRessource = nomRessource;
        Id = id;
    }

    public string NomRessource { get; }

    public int Id { get; }
}
