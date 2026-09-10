using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Vue du contexte de persistance offerte a la couche Application.
/// Elle expose les jeux d'entites et l'enregistrement, sans exposer la configuration
/// ni le fournisseur de base : c'est Infrastructure qui les detient.
/// Les regles R2 et R3 reposent sur des comptages et des tests de chevauchement
/// qui doivent rester lisibles d'un seul tenant dans le service ; les eclater en
/// methodes de repository disperserait la regle metier hors de sa couche.
/// </summary>
public interface ILocaCarDbContext
{
    DbSet<Agence> Agences { get; }
    DbSet<Client> Clients { get; }
    DbSet<CategorieVehicule> CategoriesVehicules { get; }
    DbSet<Vehicule> Vehicules { get; }
    DbSet<ReservationLocation> ReservationsLocations { get; }
    DbSet<ContratLocation> ContratsLocations { get; }

    /// <summary>
    /// Ecrit toutes les modifications suivies en une seule transaction.
    /// C'est ce point unique qui rend R3 et R4 atomiques.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
