using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ContratLocationConfiguration : BaseEntityConfiguration<ContratLocation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ContratLocation> builder)
    {
        builder.ToTable("ContratsLocations");

        builder.Property(c => c.Depart)
            .IsRequired();

        builder.Property(c => c.RetourPrevu)
            .IsRequired();

        // RetourReel et MontantFinal restent NULL tant que le vehicule n'est pas rendu :
        // c'est l'absence d'information, pas une valeur nulle.
        builder.Property(c => c.RetourReel)
            .IsRequired(false);

        builder.Property(c => c.MontantFinal)
            .IsRequired(false)
            .HasPrecision(18, 2);

        // Relation E, transaction / detail : ReservationLocation 1 - 0..1 ContratLocation.
        // C'est la FK unique cote contrat qui impose le "au plus un".
        builder.HasOne(c => c.Reservation)
            .WithOne(r => r.Contrat)
            .HasForeignKey<ContratLocation>(c => c.ReservationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.ReservationLocationId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Relation F : Vehicule 1 - * ContratLocation (historique du vehicule).
        builder.HasOne(c => c.Vehicule)
            .WithMany(v => v.Contrats)
            .HasForeignKey(c => c.VehiculeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index de travail du test de chevauchement (regles R2 et R3).
        builder.HasIndex(c => new { c.VehiculeId, c.Depart, c.RetourPrevu });
    }
}
