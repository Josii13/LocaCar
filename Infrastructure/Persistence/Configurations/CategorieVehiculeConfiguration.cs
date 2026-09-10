using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategorieVehiculeConfiguration : BaseEntityConfiguration<CategorieVehicule>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CategorieVehicule> builder)
    {
        builder.ToTable("CategoriesVehicules");

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.Libelle)
            .IsRequired()
            .HasMaxLength(100);

        // decimal(18,2) et non double : aucune erreur d'arrondi toleree en facturation (D7).
        builder.Property(c => c.TarifJournalier)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Caution)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.PenaliteRetardParJour)
            .IsRequired()
            .HasPrecision(18, 2);

        // Support du 409 attendu sur POST/PUT quand le code est deja utilise.
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
