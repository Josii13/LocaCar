using Application.Common.Exceptions;
using Application.Referentiel;
using Application.Referentiel.Dtos;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// Ecrans du referentiel CategorieVehicule (UC5).
/// Le controleur ne touche jamais EF Core : il appelle le meme service applicatif
/// que l'API, et traduit ses exceptions metier en comportements d'ecran
/// (404 pour un identifiant inconnu, message dans le formulaire pour un conflit).
/// </summary>
public class CategoriesVehiculesController : Controller
{
    private readonly ICategorieVehiculeService _service;

    public CategoriesVehiculesController(ICategorieVehiculeService service)
    {
        _service = service;
    }

    // GET /CategoriesVehicules
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _service.ObtenirToutesAsync(ct));

    // GET /CategoriesVehicules/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        try
        {
            return View(await _service.ObtenirAsync(id, ct));
        }
        catch (RessourceIntrouvableException)
        {
            return NotFound();
        }
    }

    // GET /CategoriesVehicules/Create
    public IActionResult Create() => View(new EnregistrerCategorieVehiculeDto());

    // POST /CategoriesVehicules/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EnregistrerCategorieVehiculeDto formulaire, CancellationToken ct)
    {
        // Validation de forme : "le formulaire est-il bien rempli ?"
        if (!ModelState.IsValid)
        {
            return View(formulaire);
        }

        try
        {
            var categorie = await _service.CreerAsync(formulaire, ct);
            TempData["Succes"] = $"La catégorie {categorie.Code} a été créée.";
            return RedirectToAction(nameof(Details), new { id = categorie.Id });
        }
        catch (ConflitMetierException ex)
        {
            // Regle d'etat : le code est deja pris. On reaffiche le formulaire avec la cause.
            ModelState.AddModelError(nameof(formulaire.Code), ex.Message);
            return View(formulaire);
        }
    }

    // GET /CategoriesVehicules/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            var categorie = await _service.ObtenirAsync(id, ct);
            return View(CategorieVehiculeEditionViewModel.DepuisDto(categorie));
        }
        catch (RessourceIntrouvableException)
        {
            return NotFound();
        }
    }

    // POST /CategoriesVehicules/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        [FromRoute] int id, CategorieVehiculeEditionViewModel formulaire, CancellationToken ct)
    {
        // [FromRoute] est indispensable : sans lui, le champ "Id" du formulaire poste
        // alimenterait aussi ce parametre et la comparaison serait toujours vraie.
        if (id != formulaire.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(formulaire);
        }

        try
        {
            await _service.ModifierAsync(id, formulaire, ct);
            TempData["Succes"] = $"La catégorie {formulaire.Code.Trim().ToUpperInvariant()} a été modifiée.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (RessourceIntrouvableException)
        {
            return NotFound();
        }
        catch (ConflitMetierException ex)
        {
            ModelState.AddModelError(nameof(formulaire.Code), ex.Message);
            return View(formulaire);
        }
    }

    // GET /CategoriesVehicules/Delete/5 : ecran de confirmation
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return View(await _service.ObtenirAsync(id, ct));
        }
        catch (RessourceIntrouvableException)
        {
            return NotFound();
        }
    }

    // POST /CategoriesVehicules/Delete/5 : suppression logique
    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            await _service.SupprimerAsync(id, ct);
            TempData["Succes"] = "La catégorie a été supprimée. Elle n'apparaît plus dans la liste.";
            return RedirectToAction(nameof(Index));
        }
        catch (RessourceIntrouvableException)
        {
            return NotFound();
        }
        catch (ConflitMetierException ex)
        {
            // Referentiel encore utilise : on reste sur l'ecran de confirmation avec la cause.
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await _service.ObtenirAsync(id, ct));
        }
    }
}
