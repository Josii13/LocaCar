namespace Application.Common.Interfaces;

/// <summary>
/// Fournit l'instant courant. Abstrait pour que les regles qui dependent du temps
/// (R1 "la periode est future", R3 pour la date de depart, R4 pour un retour sans
/// date fournie) puissent etre testees sans dependre de l'horloge de la machine.
/// </summary>
public interface IHorloge
{
    DateTime Maintenant { get; }
}
