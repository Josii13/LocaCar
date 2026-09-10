using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ClientConfiguration : BaseEntityConfiguration<Client>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.Property(c => c.Numero)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Nom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Telephone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.NumeroPermis)
            .IsRequired()
            .HasMaxLength(30);

        // DateOnly se projette sur le type SQL "date" : aucune partie horaire parasite.
        builder.Property(c => c.ExpirationPermis)
            .IsRequired()
            .HasColumnType("date");

        builder.HasIndex(c => c.Numero)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
