using Application.Locations;
using Application.Referentiel;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Enregistrement des cas d'utilisation. Api et Web appellent cette methode et
/// ne connaissent que les interfaces de service.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped : le service partage le DbContext de la requete en cours, ce qui est
        // la condition pour que R3 et R4 tiennent dans une seule unite de travail.
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ICategorieVehiculeService, CategorieVehiculeService>();

        return services;
    }
}
