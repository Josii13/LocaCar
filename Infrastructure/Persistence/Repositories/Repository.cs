using System.Linq.Expressions;
using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation EF Core du repository generique. Recoit le DbContext de la requete
/// par injection (Scoped) : tous les repositories d'une meme requete partagent donc
/// le meme tracker, et l'UnitOfWork n'a qu'un seul SaveChanges a appeler.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected LocaCarDbContext Contexte { get; }

    protected DbSet<TEntity> Ensemble { get; }

    public Repository(LocaCarDbContext contexte)
    {
        Contexte = contexte;
        Ensemble = contexte.Set<TEntity>();
    }

    public async Task<TEntity?> ObtenirParIdAsync(int id, CancellationToken ct = default) =>
        await Ensemble.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<TEntity>> ObtenirTousAsync(CancellationToken ct = default) =>
        await Ensemble.AsNoTracking().ToListAsync(ct);

    public Task<bool> ExisteAsync(Expression<Func<TEntity, bool>> critere, CancellationToken ct = default) =>
        Ensemble.AnyAsync(critere, ct);

    public Task<int> CompterAsync(Expression<Func<TEntity, bool>> critere, CancellationToken ct = default) =>
        Ensemble.CountAsync(critere, ct);

    public IQueryable<TEntity> Requete() => Ensemble;

    public void Ajouter(TEntity entite) => Ensemble.Add(entite);

    // Remove() ne supprime jamais physiquement : LocaCarDbContext.SaveChanges convertit
    // l'etat Deleted en IsDeleted = true (voir AppliquerAuditEtSuppressionLogique).
    public void Supprimer(TEntity entite) => Ensemble.Remove(entite);
}
