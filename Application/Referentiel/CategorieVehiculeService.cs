using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Referentiel.Dtos;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Referentiel;

/// <inheritdoc cref="ICategorieVehiculeService"/>
public class CategorieVehiculeService : ICategorieVehiculeService
{
    private readonly ILocaCarDbContext _contexte;

    /// <summary>
    /// Statuts pour lesquels une reservation empeche encore de retirer la categorie
    /// du referentiel. Terminee et Annulee sont des etats terminaux : elles n'empechent
    /// rien, elles ne sont conservees que comme historique.
    /// </summary>
    private static readonly StatutReservationLocation[] StatutsBloquants =
    [
        StatutReservationLocation.EnAttente,
        StatutReservationLocation.Confirmee,
        StatutReservationLocation.EnCours
    ];

    public CategorieVehiculeService(ILocaCarDbContext contexte)
    {
        _contexte = contexte;
    }

    public async Task<IReadOnlyList<CategorieVehiculeDto>> ObtenirToutesAsync(CancellationToken ct = default) =>
        await _contexte.CategoriesVehicules
            .OrderBy(c => c.Code)
            .Select(Projection)
            .ToListAsync(ct);

    public async Task<CategorieVehiculeDto> ObtenirAsync(int id, CancellationToken ct = default) =>
        await _contexte.CategoriesVehicules
            .Where(c => c.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(ct)
            ?? throw new RessourceIntrouvableException("Categorie de vehicule", id);

    public async Task<CategorieVehiculeDto> CreerAsync(
        EnregistrerCategorieVehiculeDto demande, CancellationToken ct = default)
    {
        var code = Normaliser(demande.Code);
        await GarantirCodeDisponibleAsync(code, null, ct);

        var categorie = new CategorieVehicule
        {
            Code = code,
            Libelle = demande.Libelle.Trim(),
            TarifJournalier = demande.TarifJournalier,
            Caution = demande.Caution,
            PenaliteRetardParJour = demande.PenaliteRetardParJour
        };

        _contexte.CategoriesVehicules.Add(categorie);
        await _contexte.SaveChangesAsync(ct);

        return VersDto(categorie);
    }

    public async Task ModifierAsync(
        int id, EnregistrerCategorieVehiculeDto demande, CancellationToken ct = default)
    {
        var categorie = await _contexte.CategoriesVehicules
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RessourceIntrouvableException("Categorie de vehicule", id);

        var code = Normaliser(demande.Code);
        await GarantirCodeDisponibleAsync(code, id, ct);

        categorie.Code = code;
        categorie.Libelle = demande.Libelle.Trim();
        categorie.TarifJournalier = demande.TarifJournalier;
        categorie.Caution = demande.Caution;
        categorie.PenaliteRetardParJour = demande.PenaliteRetardParJour;

        // Modifier un tarif ne retarife pas les contrats deja clos : leur MontantFinal
        // est fige au retour. Seules les locations futures sont concernees.
        await _contexte.SaveChangesAsync(ct);
    }

    public async Task SupprimerAsync(int id, CancellationToken ct = default)
    {
        var categorie = await _contexte.CategoriesVehicules
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RessourceIntrouvableException("Categorie de vehicule", id);

        var nbVehicules = await _contexte.Vehicules
            .CountAsync(v => v.CategorieVehiculeId == id, ct);

        var nbReservations = await _contexte.ReservationsLocations
            .CountAsync(r => r.CategorieVehiculeId == id && StatutsBloquants.Contains(r.Statut), ct);

        // C'est le pendant applicatif du Restrict pose sur les cles etrangeres :
        // on refuse explicitement plutot que de laisser la base lever une erreur brute.
        if (nbVehicules > 0 || nbReservations > 0)
        {
            throw new ConflitMetierException(
                $"La categorie {categorie.Code} est encore utilisee "
                + $"({nbVehicules} vehicule(s), {nbReservations} reservation(s) en cours) "
                + "et ne peut pas etre supprimee.");
        }

        // Remove() est converti en suppression logique par LocaCarDbContext.
        _contexte.CategoriesVehicules.Remove(categorie);
        await _contexte.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Verifie l'unicite du code parmi les categories visibles. L'index unique filtre
    /// reste le garde-fou en base ; ce controle sert a rendre un 409 explicite plutot
    /// qu'une DbUpdateException.
    /// </summary>
    private async Task GarantirCodeDisponibleAsync(string code, int? idAExclure, CancellationToken ct)
    {
        var dejaPris = await _contexte.CategoriesVehicules
            .AnyAsync(c => c.Code == code && (idAExclure == null || c.Id != idAExclure), ct);

        if (dejaPris)
        {
            throw new ConflitMetierException($"Le code {code} est deja utilise par une autre categorie.");
        }
    }

    private static string Normaliser(string code) => code.Trim().ToUpperInvariant();

    /// <summary>
    /// Projection ecrite en Expression et non en methode : EF Core doit pouvoir la
    /// traduire en SQL. Une methode ordinaire forcerait le chargement des entites
    /// completes en memoire avant le mapping.
    /// </summary>
    private static readonly Expression<Func<CategorieVehicule, CategorieVehiculeDto>> Projection =
        c => new CategorieVehiculeDto
        {
            Id = c.Id,
            Code = c.Code,
            Libelle = c.Libelle,
            TarifJournalier = c.TarifJournalier,
            Caution = c.Caution,
            PenaliteRetardParJour = c.PenaliteRetardParJour,
            NombreVehicules = c.Vehicules.Count
        };

    /// <summary>Mapping en memoire, pour une entite deja chargee ou tout juste creee.</summary>
    private static CategorieVehiculeDto VersDto(CategorieVehicule c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Libelle = c.Libelle,
        TarifJournalier = c.TarifJournalier,
        Caution = c.Caution,
        PenaliteRetardParJour = c.PenaliteRetardParJour,
        NombreVehicules = c.Vehicules.Count
    };
}
