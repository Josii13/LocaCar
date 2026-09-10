using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Horloge reelle. UTC et non heure locale : l'audit de BaseEntity est deja en UTC,
/// et une comparaison entre deux referentiels horaires differents produirait des
/// resultats faux pour R1 aux abords de minuit.
/// </summary>
public class HorlogeSysteme : IHorloge
{
    public DateTime Maintenant => DateTime.UtcNow;
}
