using System.ComponentModel.DataAnnotations;

namespace Domain.Common;

/// <summary>
/// Classe de base de toutes les entites persistees.
/// Porte l'audit, la suppression logique et le jeton de concurrence optimiste.
/// Les attributs ci-dessous declarent l'intention dans le Domain ; la configuration
/// relationnelle complete (types SQL, index, filtre global) reste en Fluent API dans
/// Infrastructure, qui fait foi pour la migration.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Cle primaire technique.</summary>
    [Key]
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
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
