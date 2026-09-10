namespace Application.Common.Exceptions;

/// <summary>
/// La demande elle-meme est irrecevable : periode incoherente, permis expire,
/// date de retour anterieure au depart. Correspond au 400 Bad Request.
/// A ne pas confondre avec <see cref="ConflitMetierException"/> : ici la requete
/// est fautive, la ou le conflit vient de l'etat du systeme.
/// </summary>
public class DemandeInvalideException : Exception
{
    public DemandeInvalideException(string message) : base(message)
    {
    }
}
