namespace Domain.Common;

/// <summary>
/// Classe de base de toutes les entites persistees.
/// Porte l'audit, la suppression logique et le jeton de concurrence optimiste.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Cle primaire technique.</summary>
    public int Id { get; set; }

    /// <summary>Date de creation de la ligne.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date de derniere modification. Null tant que la ligne n'a jamais ete modifiee.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Marque de suppression logique. Un filtre global exclut les lignes a true
    /// de toutes les requetes.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>Date de la suppression logique. Null tant que la ligne est active.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Jeton de concurrence optimiste (rowversion SQL Server).
    /// Genere par la base, jamais affecte par le code applicatif.
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
