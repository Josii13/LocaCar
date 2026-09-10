using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Point d'entree unique d'enregistrement de la couche Infrastructure.
/// Les projets Api et Web n'ont ainsi jamais a connaitre le fournisseur de base
/// ni le nom du DbContext.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var chaine = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Chaine de connexion 'DefaultConnection' absente de la configuration.");

        services.AddDbContext<LocaCarDbContext>(options => options.UseSqlServer(chaine));

        // La couche Application ne voit que l'interface, jamais la classe concrete.
        services.AddScoped<ILocaCarDbContext>(sp => sp.GetRequiredService<LocaCarDbContext>());

        services.AddSingleton<IHorloge, HorlogeSysteme>();

        return services;
    }
}
