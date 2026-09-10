using Application.Locations;
using Application.Locations.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>UC4 — cloture d'une location et calcul du montant final.</summary>
[ApiController]
[Route("api/contrats")]
public class ContratsController : ControllerBase
{
    private readonly ILocationService _service;

    public ContratsController(ILocationService service)
    {
        _service = service;
    }

    /// <summary>Cible de l'en-tete Location renvoye par le demarrage d'une location.</summary>
    [HttpGet("{id:int}", Name = nameof(ObtenirContrat))]
    [ProducesResponseType(typeof(ContratDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContratDto>> ObtenirContrat(int id, CancellationToken ct)
        => Ok(await _service.ObtenirContratAsync(id, ct));

    /// <summary>
    /// UC4 — enregistre le retour et calcule le montant final (R4).
    /// Repond 200 et non 201 : aucune ressource n'est creee, un contrat existant
    /// change d'etat. Le corps est facultatif ; sans date, l'instant courant est pris.
    /// </summary>
    [HttpPost("{id:int}/retour")]
    [ProducesResponseType(typeof(ContratDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContratDto>> EnregistrerRetour(
        int id, [FromBody] EnregistrerRetourDto? demande, CancellationToken ct)
    {
        var contrat = await _service.EnregistrerRetourAsync(id, demande?.RetourReel, ct);
        return Ok(contrat);
    }
}
