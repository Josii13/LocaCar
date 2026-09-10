using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>Requetes propres au referentiel des categories.</summary>
public interface ICategorieVehiculeRepository : IRepository<CategorieVehicule>
{
    /// <summary>
    /// Vrai si une categorie visible, autre que <paramref name="idAExclure"/>, porte deja
    /// ce code. Support applicatif du 409 sur POST et PUT ; l'index unique filtre reste
    /// le garde-fou en base.
    /// </summary>
    Task<bool> CodeDejaUtiliseAsync(string code, int? idAExclure, CancellationToken ct = default);
}
