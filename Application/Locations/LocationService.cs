using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Locations.Dtos;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Locations;

/// <inheritdoc cref="ILocationService"/>
public class LocationService : ILocationService
{
    private readonly ILocaCarDbContext _contexte;
    private readonly IHorloge _horloge;

    /// <summary>Statuts qui consomment une unite de capacite dans leur categorie (R2).</summary>
    private static readonly StatutReservationLocation[] StatutsActifs =
    [
        StatutReservationLocation.EnAttente,
        StatutReservationLocation.Confirmee
    ];

    public LocationService(ILocaCarDbContext contexte, IHorloge horloge)
    {
        _contexte = contexte;
        _horloge = horloge;
    }

    // ------------------------------------------------------------------
    // UC1 — vehicules disponibles
    // ------------------------------------------------------------------

    public async Task<IReadOnlyList<VehiculeDisponibleDto>> ObtenirVehiculesDisponiblesAsync(
        int categorieVehiculeId, DateTime debut, DateTime fin, CancellationToken ct = default)
    {
        if (fin <= debut)
        {
            throw new DemandeInvalideException("La date de fin doit etre posterieure a la date de debut.");
        }

        await GarantirCategorieExistanteAsync(categorieVehiculeId, ct);

        return await VehiculesLibres(categorieVehiculeId, debut, fin)
            .Select(v => new VehiculeDisponibleDto
            {
                Id = v.Id,
                Immatriculation = v.Immatriculation,
                Modele = v.Modele,
                CategorieCode = v.Categorie.Code,
                AgenceCode = v.Agence.Code,
                AgenceVille = v.Agence.Ville
            })
            .ToListAsync(ct);
    }

    // ------------------------------------------------------------------
    // Lectures
    // ------------------------------------------------------------------

    public async Task<ReservationDto> ObtenirReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _contexte.ReservationsLocations
            .Include(r => r.Client)
            .Include(r => r.Categorie)
            .Include(r => r.Contrat)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new RessourceIntrouvableException("Reservation", id);

        return VersDto(reservation);
    }

    public async Task<ContratDto> ObtenirContratAsync(int id, CancellationToken ct = default)
    {
        var contrat = await _contexte.ContratsLocations
            .Include(c => c.Vehicule)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RessourceIntrouvableException("Contrat", id);

        return VersDto(contrat);
    }

    // ------------------------------------------------------------------
    // UC2 — creation de reservation : R1, puis R2, puis R4.1
    // ------------------------------------------------------------------

    public async Task<ReservationDto> CreerReservationAsync(
        CreerReservationDto demande, CancellationToken ct = default)
    {
        // --- R1, conditions 1 et 2 : elles ne dependent d'aucune donnee en base.
        if (demande.Debut <= _horloge.Maintenant)
        {
            throw new DemandeInvalideException("La periode de location doit etre future.");
        }

        if (demande.Fin <= demande.Debut)
        {
            throw new DemandeInvalideException("La date de fin doit etre posterieure a la date de debut.");
        }

        var client = await _contexte.Clients
            .FirstOrDefaultAsync(c => c.Id == demande.ClientId, ct)
            ?? throw new RessourceIntrouvableException("Client", demande.ClientId);

        var categorie = await _contexte.CategoriesVehicules
            .FirstOrDefaultAsync(c => c.Id == demande.CategorieVehiculeId, ct)
            ?? throw new RessourceIntrouvableException("Categorie de vehicule", demande.CategorieVehiculeId);

        // --- R1, condition 3 : le permis doit rester valide jusqu'au retour prevu.
        if (client.ExpirationPermis < DateOnly.FromDateTime(demande.Fin))
        {
            throw new DemandeInvalideException(
                $"Le permis du client expire le {client.ExpirationPermis:dd/MM/yyyy}, "
                + $"avant la fin de la location prevue le {demande.Fin:dd/MM/yyyy}.");
        }

        // --- R2 : capacite de la categorie sur la periode.
        var nbVehiculesLibres = await VehiculesLibres(demande.CategorieVehiculeId, demande.Debut, demande.Fin)
            .CountAsync(ct);

        var nbReservationsActives = await ReservationsActivesChevauchantes(
            demande.CategorieVehiculeId, demande.Debut, demande.Fin).CountAsync(ct);

        // Une reservation active n'a pas encore de vehicule affecte : elle occupe une
        // unite de capacite. Sans ce comptage, N clients reserveraient la meme voiture.
        if (nbVehiculesLibres <= nbReservationsActives)
        {
            throw new ConflitMetierException(
                $"Plus de capacite sur la categorie {categorie.Code} pour cette periode "
                + $"({nbVehiculesLibres} vehicule(s) libre(s) pour {nbReservationsActives} reservation(s) active(s)).");
        }

        // --- R4.1 : le montant estime est calcule ici, jamais recu du client.
        var joursEstimes = NombreDeJours(demande.Debut, demande.Fin);

        var reservation = new ReservationLocation
        {
            ClientId = client.Id,
            CategorieVehiculeId = categorie.Id,
            Debut = demande.Debut,
            Fin = demande.Fin,
            Statut = StatutReservationLocation.EnAttente,
            MontantEstime = joursEstimes * categorie.TarifJournalier
        };

        _contexte.ReservationsLocations.Add(reservation);
        await _contexte.SaveChangesAsync(ct);

        reservation.Client = client;
        reservation.Categorie = categorie;
        return VersDto(reservation);
    }

    // ------------------------------------------------------------------
    // UC3 — demarrage de la location (R3)
    // ------------------------------------------------------------------

    public async Task<ContratDto> DemarrerLocationAsync(int reservationId, CancellationToken ct = default)
    {
        var reservation = await _contexte.ReservationsLocations
            .Include(r => r.Categorie)
            .FirstOrDefaultAsync(r => r.Id == reservationId, ct)
            ?? throw new RessourceIntrouvableException("Reservation", reservationId);

        if (!StatutsActifs.Contains(reservation.Statut))
        {
            throw new ConflitMetierException(
                $"Une reservation au statut {reservation.Statut} ne peut pas etre demarree.");
        }

        // Le vehicule n'est choisi qu'ici : on reserve une categorie, on contractualise
        // un vehicule. La disponibilite est reevaluee maintenant, pas a la reservation.
        var vehicule = await VehiculesLibres(reservation.CategorieVehiculeId, reservation.Debut, reservation.Fin)
            .OrderBy(v => v.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new ConflitMetierException(
                "Aucun vehicule libre dans cette categorie pour la periode de la reservation.");

        var contrat = new ContratLocation
        {
            ReservationLocationId = reservation.Id,
            VehiculeId = vehicule.Id,
            Depart = _horloge.Maintenant,
            RetourPrevu = reservation.Fin
        };

        _contexte.ContratsLocations.Add(contrat);
        vehicule.Statut = StatutVehicule.Loue;
        reservation.Statut = StatutReservationLocation.EnCours;

        // Les trois effets partent dans un seul SaveChanges, donc une seule transaction :
        // soit le contrat, le statut du vehicule et celui de la reservation changent
        // ensemble, soit rien ne change.
        await _contexte.SaveChangesAsync(ct);

        contrat.Vehicule = vehicule;
        return VersDto(contrat);
    }

    // ------------------------------------------------------------------
    // UC4 — retour et montant final (R4.2)
    // ------------------------------------------------------------------

    public async Task<ContratDto> EnregistrerRetourAsync(
        int contratId, DateTime? retourReel, CancellationToken ct = default)
    {
        var contrat = await _contexte.ContratsLocations
            .Include(c => c.Vehicule)
            .Include(c => c.Reservation)
                .ThenInclude(r => r.Categorie)
            .FirstOrDefaultAsync(c => c.Id == contratId, ct)
            ?? throw new RessourceIntrouvableException("Contrat", contratId);

        if (contrat.RetourReel is not null)
        {
            throw new ConflitMetierException(
                $"Le retour de ce contrat a deja ete enregistre le {contrat.RetourReel:dd/MM/yyyy HH:mm}.");
        }

        var dateRetour = retourReel ?? _horloge.Maintenant;

        if (dateRetour < contrat.Depart)
        {
            throw new DemandeInvalideException(
                "La date de retour ne peut pas etre anterieure a la date de depart.");
        }

        var categorie = contrat.Reservation.Categorie;

        // Toute journee entamee est due : Ceiling, pas d'arrondi au plus proche.
        var joursFactures = NombreDeJours(contrat.Depart, dateRetour);

        // Le retard est facture deux fois, et c'est voulu : les jours sont dus car le
        // vehicule est reellement immobilise, la penalite s'y ajoute comme sanction (D3).
        var joursRetard = dateRetour > contrat.RetourPrevu
            ? NombreDeJours(contrat.RetourPrevu, dateRetour)
            : 0m;

        contrat.RetourReel = dateRetour;
        contrat.MontantFinal = joursFactures * categorie.TarifJournalier
                             + joursRetard * categorie.PenaliteRetardParJour;
        contrat.Vehicule.Statut = StatutVehicule.Disponible;
        contrat.Reservation.Statut = StatutReservationLocation.Terminee;

        // La aussi, un seul SaveChanges pour les quatre effets.
        await _contexte.SaveChangesAsync(ct);

        return VersDto(contrat);
    }

    // ------------------------------------------------------------------
    // Briques communes
    // ------------------------------------------------------------------

    /// <summary>
    /// Vehicules d'une categorie affectables sur la periode : statut Disponible et
    /// aucun contrat chevauchant. Definition unique de "vehicule libre", partagee par
    /// UC1, par le comptage de R2 et par la selection de R3.
    /// Un contrat immobilise le vehicule depuis Depart jusqu'au retour reel s'il a eu
    /// lieu, jusqu'au retour prevu sinon.
    /// Bornes exclusives : une location finissant a 08h00 ne chevauche pas une location
    /// commencant a 08h00.
    /// </summary>
    private IQueryable<Vehicule> VehiculesLibres(int categorieVehiculeId, DateTime debut, DateTime fin) =>
        _contexte.Vehicules
            .Where(v => v.CategorieVehiculeId == categorieVehiculeId
                        && v.Statut == StatutVehicule.Disponible
                        && !v.Contrats.Any(c => c.Depart < fin
                                                && debut < (c.RetourReel ?? c.RetourPrevu)));

    /// <summary>
    /// Reservations de la categorie qui occupent de la capacite sur la periode :
    /// statut EnAttente ou Confirmee, et periode chevauchante.
    /// </summary>
    private IQueryable<ReservationLocation> ReservationsActivesChevauchantes(
        int categorieVehiculeId, DateTime debut, DateTime fin) =>
        _contexte.ReservationsLocations
            .Where(r => r.CategorieVehiculeId == categorieVehiculeId
                        && StatutsActifs.Contains(r.Statut)
                        && r.Debut < fin
                        && debut < r.Fin);

    private async Task GarantirCategorieExistanteAsync(int categorieVehiculeId, CancellationToken ct)
    {
        var existe = await _contexte.CategoriesVehicules
            .AnyAsync(c => c.Id == categorieVehiculeId, ct);

        if (!existe)
        {
            throw new RessourceIntrouvableException("Categorie de vehicule", categorieVehiculeId);
        }
    }

    /// <summary>
    /// Nombre de journees entamees entre deux instants. TotalDays est fractionnaire
    /// (2,25 jours) ; Ceiling le porte au jour superieur (3). C'est la raison d'etre
    /// du choix de DateTime plutot que DateOnly pour les periodes.
    /// </summary>
    private static decimal NombreDeJours(DateTime debut, DateTime fin) =>
        (decimal)Math.Ceiling((fin - debut).TotalDays);

    private static ReservationDto VersDto(ReservationLocation r) => new()
    {
        Id = r.Id,
        ClientId = r.ClientId,
        ClientNom = r.Client?.Nom ?? string.Empty,
        CategorieVehiculeId = r.CategorieVehiculeId,
        CategorieCode = r.Categorie?.Code ?? string.Empty,
        Debut = r.Debut,
        Fin = r.Fin,
        Statut = r.Statut.ToString(),
        MontantEstime = r.MontantEstime,
        ContratId = r.Contrat?.Id
    };

    private static ContratDto VersDto(ContratLocation c) => new()
    {
        Id = c.Id,
        ReservationLocationId = c.ReservationLocationId,
        VehiculeId = c.VehiculeId,
        VehiculeImmatriculation = c.Vehicule?.Immatriculation ?? string.Empty,
        Depart = c.Depart,
        RetourPrevu = c.RetourPrevu,
        RetourReel = c.RetourReel,
        MontantFinal = c.MontantFinal,
        JoursFactures = c.RetourReel is null
            ? null
            : (int)NombreDeJours(c.Depart, c.RetourReel.Value),
        JoursRetard = c.RetourReel is null
            ? null
            : c.RetourReel.Value > c.RetourPrevu
                ? (int)NombreDeJours(c.RetourPrevu, c.RetourReel.Value)
                : 0
    };
}
