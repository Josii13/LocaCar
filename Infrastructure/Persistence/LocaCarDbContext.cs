using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core de LocaCar.
/// Aucune contrainte n'est declaree par attribut sur les entites : toute la
/// configuration relationnelle vit dans les classes de Persistence/Configurations,
/// ce qui laisse le projet Domain sans dependance a EF Core.
/// </summary>
public class LocaCarDbContext : DbContext, ILocaCarDbContext
{
    public LocaCarDbContext(DbContextOptions<LocaCarDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agence> Agences => Set<Agence>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<CategorieVehicule> CategoriesVehicules => Set<CategorieVehicule>();
    public DbSet<Vehicule> Vehicules => Set<Vehicule>();
    public DbSet<ReservationLocation> ReservationsLocations => Set<ReservationLocation>();
    public DbSet<ContratLocation> ContratsLocations => Set<ContratLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Charge les six IEntityTypeConfiguration de cet assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocaCarDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        AppliquerAuditEtSuppressionLogique();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AppliquerAuditEtSuppressionLogique();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Renseigne les colonnes d'audit et transforme toute suppression physique
    /// demandee au tracker en suppression logique.
    /// Centralise ici pour qu'aucun service applicatif n'ait a y penser.
    /// </summary>
    private void AppliquerAuditEtSuppressionLogique()
    {
        var maintenant = DateTime.UtcNow;

        foreach (var entree in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entree.State)
            {
                case EntityState.Added:
                    entree.Entity.CreatedAt = maintenant;
                    entree.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entree.Entity.UpdatedAt = maintenant;
                    // CreatedAt est immuable : on empeche toute reecriture accidentelle.
                    entree.Property(e => e.CreatedAt).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Remove() ne supprime jamais la ligne : elle est marquee supprimee.
                    entree.State = EntityState.Modified;
                    entree.Entity.IsDeleted = true;
                    entree.Entity.DeletedAt = maintenant;
                    entree.Entity.UpdatedAt = maintenant;
                    entree.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}
