using Application.Referentiel.Dtos;

namespace Application.Referentiel;

/// <summary>
/// CRUD du referentiel tarifaire (UC5). Partage par l'API et par les ecrans MVC :
/// les deux presentations appellent le meme service, aucune ne parle a EF Core.
/// </summary>
public interface ICategorieVehiculeService
{
    Task<IReadOnlyList<CategorieVehiculeDto>> ObtenirToutesAsync(CancellationToken ct = default);

    Task<CategorieVehiculeDto> ObtenirAsync(int id, CancellationToken ct = default);

    Task<CategorieVehiculeDto> CreerAsync(EnregistrerCategorieVehiculeDto demande, CancellationToken ct = default);

    Task ModifierAsync(int id, EnregistrerCategorieVehiculeDto demande, CancellationToken ct = default);

    /// <summary>Suppression logique. Refusee si la categorie est encore referencee.</summary>
    Task SupprimerAsync(int id, CancellationToken ct = default);
}
