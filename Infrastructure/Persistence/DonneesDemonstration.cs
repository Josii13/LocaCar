using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

/// <summary>
/// Jeu de donnees minimal pour la demonstration (jalon 2 du sujet). Applique les
/// migrations puis insere les lignes de reference si la base est vide.
/// Idempotent : relancer l'application ne duplique rien.
/// Sur une base fraiche, les identifiants obtenus sont ceux utilises par Api/Api.http.
/// </summary>
public static class DonneesDemonstration
{
    public static async Task InitialiserBaseDeDemonstrationAsync(
        this IServiceProvider services, CancellationToken ct = default)
    {
        using var portee = services.CreateScope();
        var contexte = portee.ServiceProvider.GetRequiredService<LocaCarDbContext>();

        await contexte.Database.MigrateAsync(ct);

        if (await contexte.CategoriesVehicules.AnyAsync(ct))
        {
            return;
        }

        // Agences : Id 1 et 2
        var plateau = new Agence { Code = "ABJ-PLT", Nom = "LocaCar Plateau", Ville = "Abidjan", Adresse = "Avenue Franchet d'Esperey, Plateau" };
        var cocody = new Agence { Code = "ABJ-COC", Nom = "LocaCar Cocody", Ville = "Abidjan", Adresse = "Boulevard Latrille, Cocody" };

        // Clients : Id 1 (permis valide longtemps) et Id 2 (permis qui expire bientot,
        // sert a demontrer le refus R1 "permis expire avant le retour prevu").
        var aya = new Client { Numero = "CL-0001", Nom = "Kouassi Aya", Telephone = "+225 07 01 02 03 04", NumeroPermis = "CI-PERM-458712", ExpirationPermis = new DateOnly(2029, 6, 30) };
        var moussa = new Client { Numero = "CL-0002", Nom = "Traore Moussa", Telephone = "+225 05 11 12 13 14", NumeroPermis = "CI-PERM-120987", ExpirationPermis = new DateOnly(2026, 9, 30) };

        // Categories : Id 1 ECO (3 vehicules), Id 2 SUV (1 vehicule), Id 3 LUX (aucun vehicule,
        // donc supprimable : sert a demontrer la suppression logique).
        var eco = new CategorieVehicule { Code = "ECO", Libelle = "Economique", TarifJournalier = 25_000m, Caution = 100_000m, PenaliteRetardParJour = 10_000m };
        var suv = new CategorieVehicule { Code = "SUV", Libelle = "SUV et 4x4", TarifJournalier = 60_000m, Caution = 300_000m, PenaliteRetardParJour = 20_000m };
        var lux = new CategorieVehicule { Code = "LUX", Libelle = "Berline de luxe", TarifJournalier = 120_000m, Caution = 1_000_000m, PenaliteRetardParJour = 50_000m };

        // Vehicules : Id 1 a 4. Le troisieme est en maintenance : il ne compte jamais
        // comme libre. ECO a donc 2 unites de capacite, SUV une seule.
        var vehicules = new[]
        {
            new Vehicule { Immatriculation = "AA-123-BB", Modele = "Toyota Yaris", Statut = StatutVehicule.Disponible, Categorie = eco, Agence = plateau },
            new Vehicule { Immatriculation = "AA-456-CC", Modele = "Hyundai i10", Statut = StatutVehicule.Disponible, Categorie = eco, Agence = plateau },
            new Vehicule { Immatriculation = "AA-789-DD", Modele = "Suzuki Swift", Statut = StatutVehicule.Maintenance, Categorie = eco, Agence = cocody },
            new Vehicule { Immatriculation = "BB-111-EE", Modele = "Toyota RAV4", Statut = StatutVehicule.Disponible, Categorie = suv, Agence = cocody }
        };

        contexte.Agences.AddRange(plateau, cocody);
        contexte.Clients.AddRange(aya, moussa);
        contexte.CategoriesVehicules.AddRange(eco, suv, lux);
        contexte.Vehicules.AddRange(vehicules);

        await contexte.SaveChangesAsync(ct);
    }
}
