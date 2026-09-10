using System.Globalization;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Messages de liaison de modele en francais : ce sont ceux que MVC produit lui-meme
// quand une valeur est absente ou illisible, avant meme les attributs de validation.
builder.Services.AddControllersWithViews(options =>
{
    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(_ => "Ce champ est obligatoire.");
    messages.SetMissingBindRequiredValueAccessor(champ => $"Le champ {champ} est obligatoire.");
    messages.SetAttemptedValueIsInvalidAccessor((valeur, champ) => $"La valeur « {valeur} » n'est pas valide pour {champ}.");
    messages.SetUnknownValueIsInvalidAccessor(champ => $"La valeur saisie n'est pas valide pour {champ}.");
    messages.SetValueIsInvalidAccessor(valeur => $"La valeur « {valeur} » n'est pas valide.");
    messages.SetValueMustBeANumberAccessor(champ => $"Le champ {champ} doit etre un nombre.");
    messages.SetNonPropertyAttemptedValueIsInvalidAccessor(valeur => $"La valeur « {valeur} » n'est pas valide.");
    messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "La valeur saisie n'est pas valide.");
    messages.SetNonPropertyValueMustBeANumberAccessor(() => "La valeur doit etre un nombre.");
    messages.SetMissingKeyOrValueAccessor(() => "Une valeur est requise.");
    messages.SetMissingRequestBodyRequiredValueAccessor(() => "Le corps de la requete est obligatoire.");
});

// Meme DbContext, memes repositories, memes services que l'API : le projet Web ne
// connait que les interfaces de la couche Application.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Culture francaise pour l'affichage des montants et des dates.
var cultureFr = new CultureInfo("fr-FR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureFr),
    SupportedCultures = [cultureFr],
    SupportedUICultures = [cultureFr]
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnet/core-hsts.
    app.UseHsts();
}
else
{
    // Migrations appliquees et jeu de demonstration insere si la base est vide.
    await app.Services.InitialiserBaseDeDemonstrationAsync();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
