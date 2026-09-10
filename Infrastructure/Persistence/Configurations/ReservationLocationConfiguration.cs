using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReservationLocationConfiguration : BaseEntityConfiguration<ReservationLocation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReservationLocation> builder)
    {
        builder.ToTable("ReservationsLocations");

        builder.Property(r => r.Debut)
            .IsRequired();

        builder.Property(r => r.Fin)
            .IsRequired();

        builder.Property(r => r.Statut)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.MontantEstime)
            .IsRequired()
            .HasPrecision(18, 2);

        // Relation C : Client 1 - * ReservationLocation.
        builder.HasOne(r => r.Client)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation D : CategorieVehicule 1 - * ReservationLocation.
        // On reserve une categorie, jamais un vehicule : aucune FK vers Vehicule ici.
        builder.HasOne(r => r.Categorie)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CategorieVehiculeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index de travail de la regle R2 : le comptage des reservations actives
        // filtre sur la categorie, le statut et la periode.
        builder.HasIndex(r => new { r.CategorieVehiculeId, r.Statut, r.Debut, r.Fin });
    }
}
