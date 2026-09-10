using Application.Referentiel;
using Application.Referentiel.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>UC5 — CRUD du referentiel tarifaire.</summary>
[ApiController]
[Route("api/categories-vehicules")]
public class CategoriesVehiculesController : ControllerBase
{
    private readonly ICategorieVehiculeService _service;

    public CategoriesVehiculesController(ICategorieVehiculeService service)
    {
        _service = service;
    }

    /// <summary>Les categories supprimees logiquement ne figurent pas dans la liste.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategorieVehiculeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategorieVehiculeDto>>> Lister(CancellationToken ct)
        => Ok(await _service.ObtenirToutesAsync(ct));

    [HttpGet("{id:int}", Name = nameof(ObtenirCategorie))]
    [ProducesResponseType(typeof(CategorieVehiculeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategorieVehiculeDto>> ObtenirCategorie(int id, CancellationToken ct)
        => Ok(await _service.ObtenirAsync(id, ct));

    /// <summary>409 si le code est deja porte par une categorie visible.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategorieVehiculeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategorieVehiculeDto>> Creer(
        [FromBody] EnregistrerCategorieVehiculeDto demande, CancellationToken ct)
    {
        var categorie = await _service.CreerAsync(demande, ct);

        return CreatedAtRoute(nameof(ObtenirCategorie), new { id = categorie.Id }, categorie);
    }

    /// <summary>204 : la modification a reussi et il n'y a rien a renvoyer.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Modifier(
        int id, [FromBody] EnregistrerCategorieVehiculeDto demande, CancellationToken ct)
    {
        await _service.ModifierAsync(id, demande, ct);
        return NoContent();
    }

    /// <summary>
    /// Suppression logique. 409 si la categorie est encore referencee par un vehicule
    /// ou par une reservation non terminee.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Supprimer(int id, CancellationToken ct)
    {
        await _service.SupprimerAsync(id, ct);
        return NoContent();
    }
}
