using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AgenceConfiguration : BaseEntityConfiguration<Agence>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Agence> builder)
    {
        builder.ToTable("Agences");

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.Nom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Ville)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(a => a.Adresse)
            .IsRequired()
            .HasMaxLength(200);

        // Index unique filtre : un code libere par une suppression logique
        // doit pouvoir etre reutilise.
        builder.HasIndex(a => a.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
