namespace Application.Common.Exceptions;

/// <summary>
/// La demande est bien formee mais l'etat du systeme l'interdit : plus de capacite
/// sur la categorie (R2), transition de statut impossible (R3), retour deja
/// enregistre (R4), referentiel encore reference.
/// Correspond au 409 Conflict.
/// </summary>
public class ConflitMetierException : Exception
{
    public ConflitMetierException(string message) : base(message)
    {
    }
}
