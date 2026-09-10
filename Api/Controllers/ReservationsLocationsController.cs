using Application.Locations;
using Application.Locations.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>UC2 et UC3 — creation d'une reservation et demarrage de la location.</summary>
[ApiController]
[Route("api/reservations-locations")]
public class ReservationsLocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public ReservationsLocationsController(ILocationService service)
    {
        _service = service;
    }

    /// <summary>Cible de l'en-tete Location renvoye a la creation.</summary>
    [HttpGet("{id:int}", Name = nameof(ObtenirReservation))]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> ObtenirReservation(int id, CancellationToken ct)
        => Ok(await _service.ObtenirReservationAsync(id, ct));

    /// <summary>
    /// UC2 — cree une reservation. Le corps ne contient pas MontantEstime :
    /// il est calcule par le serveur (R4.1) et renvoye dans la reponse.
    /// 400 si R1 est violee, 404 si un identifiant est inconnu, 409 si R2 l'est.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Creer(
        [FromBody] CreerReservationDto demande, CancellationToken ct)
    {
        var reservation = await _service.CreerReservationAsync(demande, ct);

        return CreatedAtRoute(
            nameof(ObtenirReservation),
            new { id = reservation.Id },
            reservation);
    }

    /// <summary>
    /// UC3 — demarre la location : affectation d'un vehicule et creation du contrat,
    /// dans une seule unite de travail (R3). L'en-tete Location pointe vers le contrat cree.
    /// </summary>
    [HttpPost("{id:int}/demarrer")]
    [ProducesResponseType(typeof(ContratDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContratDto>> Demarrer(int id, CancellationToken ct)
    {
        var contrat = await _service.DemarrerLocationAsync(id, ct);

        return CreatedAtRoute(
            nameof(ContratsController.ObtenirContrat),
            new { id = contrat.Id },
            contrat);
    }
}
