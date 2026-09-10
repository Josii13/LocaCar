using Application.Locations;
using Application.Locations.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>UC1 — consultation du parc affectable.</summary>
[ApiController]
[Route("api/vehicules")]
public class VehiculesController : ControllerBase
{
    private readonly ILocationService _service;

    public VehiculesController(ILocationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Vehicules d'une categorie affectables sur la periode.
    /// Le controleur ne fait que transmettre : la definition de "disponible" appartient
    /// au service, sinon elle divergerait de celle utilisee par R2 et R3.
    /// </summary>
    [HttpGet("disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<VehiculeDisponibleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<VehiculeDisponibleDto>>> ObtenirDisponibles(
        [FromQuery] int categorieId,
        [FromQuery] DateTime debut,
        [FromQuery] DateTime fin,
        CancellationToken ct)
    {
        var vehicules = await _service.ObtenirVehiculesDisponiblesAsync(categorieId, debut, fin, ct);
        return Ok(vehicules);
    }
}
