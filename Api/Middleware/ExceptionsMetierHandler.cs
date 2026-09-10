using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

/// <summary>
/// Traduit les exceptions metier en codes HTTP, en un seul endroit.
/// Consequence directe : aucun controleur n'ecrit de try/catch ni ne decide d'un
/// code d'erreur. Le type de l'exception suffit a determiner la reponse, ce qui
/// rend impossible qu'une meme regle soit rendue en 400 ici et en 409 ailleurs.
/// </summary>
public class ExceptionsMetierHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ExceptionsMetierHandler> _logger;

    public ExceptionsMetierHandler(
        IProblemDetailsService problemDetails,
        ILogger<ExceptionsMetierHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexte, Exception exception, CancellationToken ct)
    {
        var (statut, titre) = exception switch
        {
            // La requete est fautive : R1 violee, periode incoherente, retour avant depart.
            DemandeInvalideException => (StatusCodes.Status400BadRequest, "Demande invalide"),

            // L'identifiant ne designe rien de visible.
            RessourceIntrouvableException => (StatusCodes.Status404NotFound, "Ressource introuvable"),

            // La requete est bien formee, c'est l'etat du systeme qui l'interdit :
            // R2, transition de statut, retour deja enregistre, referentiel utilise.
            ConflitMetierException => (StatusCodes.Status409Conflict, "Conflit metier"),

            _ => (0, string.Empty)
        };

        // Toute autre exception est une anomalie technique : on ne la deguise pas
        // en erreur metier, elle remonte et produit un 500.
        if (statut == 0)
        {
            return false;
        }

        _logger.LogInformation(
            "Regle metier refusee ({Statut}) : {Message}", statut, exception.Message);

        contexte.Response.StatusCode = statut;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexte,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statut,
                Title = titre,
                Detail = exception.Message,
                Instance = $"{contexte.Request.Method} {contexte.Request.Path}"
            }
        });
    }
}
