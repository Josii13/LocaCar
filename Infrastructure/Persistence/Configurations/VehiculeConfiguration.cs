using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VehiculeConfiguration : BaseEntityConfiguration<Vehicule>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Vehicule> builder)
    {
        builder.ToTable("Vehicules");

        builder.Property(v => v.Immatriculation)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Modele)
            .IsRequired()
            .HasMaxLength(100);

        // L'enum est stocke en int : valeur stable, independante du libelle C#.
        builder.Property(v => v.Statut)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(v => v.Immatriculation)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Relation B : CategorieVehicule 1 - * Vehicule.
        // Restrict et non Cascade : la suppression est logique, et c'est ce Restrict
        // qui justifie le 409 sur DELETE d'une categorie encore utilisee.
        builder.HasOne(v => v.Categorie)
            .WithMany(c => c.Vehicules)
            .HasForeignKey(v => v.CategorieVehiculeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation A : Agence 1 - * Vehicule.
        builder.HasOne(v => v.Agence)
            .WithMany(a => a.Vehicules)
            .HasForeignKey(v => v.AgenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
