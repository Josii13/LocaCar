using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration commune a toutes les entites : cle primaire, jeton de concurrence
/// et filtre global de suppression logique.
/// Chaque entite n'a plus qu'a decrire ce qui lui est propre dans ConfigureEntity.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // rowversion SQL Server : la base incremente la valeur a chaque UPDATE,
        // EF Core l'ajoute au WHERE et leve DbUpdateConcurrencyException si 0 ligne touchee.
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Filtre global : les lignes supprimees logiquement sortent de toutes les requetes,
        // y compris des chargements de navigation.
        builder.HasQueryFilter(e => !e.IsDeleted);

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
