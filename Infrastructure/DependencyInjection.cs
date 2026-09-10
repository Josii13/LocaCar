using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
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

        // AddDbContext enregistre le contexte en Scoped : une instance par requete HTTP.
        services.AddDbContext<LocaCarDbContext>(options => options.UseSqlServer(chaine));

        // Repositories et UnitOfWork sont Scoped eux aussi : ils recoivent tous la meme
        // instance de LocaCarDbContext, condition de l'atomicite de R3 et R4.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICategorieVehiculeRepository, CategorieVehiculeRepository>();
        services.AddScoped<IVehiculeRepository, VehiculeRepository>();
        services.AddScoped<IReservationLocationRepository, ReservationLocationRepository>();
        services.AddScoped<IContratLocationRepository, ContratLocationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IHorloge, HorlogeSysteme>();

        return services;
    }
}
