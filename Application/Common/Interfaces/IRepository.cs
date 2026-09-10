using System.Linq.Expressions;
using Domain.Common;

namespace Application.Common.Interfaces;

/// <summary>
/// Acces generique aux entites persistees. Aucune methode n'enregistre : c'est
/// <see cref="IUnitOfWork.SaveChangesAsync"/> qui ecrit, une seule fois, a la fin
/// d'un cas d'utilisation reussi. C'est ce qui rend R3 et R4 atomiques.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>Entite suivie par le contexte, ou null si l'identifiant ne designe rien de visible.</summary>
    Task<TEntity?> ObtenirParIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<TEntity>> ObtenirTousAsync(CancellationToken ct = default);

    Task<bool> ExisteAsync(Expression<Func<TEntity, bool>> critere, CancellationToken ct = default);

    Task<int> CompterAsync(Expression<Func<TEntity, bool>> critere, CancellationToken ct = default);

    /// <summary>
    /// Point de depart d'une requete composable (projection, tri, filtre). Le filtre
    /// global de suppression logique s'applique deja.
    /// </summary>
    IQueryable<TEntity> Requete();

    /// <summary>Marque l'entite pour insertion. Rien n'est ecrit avant SaveChangesAsync.</summary>
    void Ajouter(TEntity entite);

    /// <summary>
    /// Marque l'entite pour suppression. Le contexte convertit cette demande en
    /// suppression logique (IsDeleted = true) au moment de SaveChangesAsync.
    /// </summary>
    void Supprimer(TEntity entite);
}
