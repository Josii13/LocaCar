# LocaCar — gestion de location de véhicules

| | |
|---|---|
| Étudiant | Josias Caleb N'Cho |
| Formation | Master 2 RIGL — 2026 |
| Sujet | Fiche 5.4 « LocaCar » du fil rouge .NET : réserver un véhicule par catégorie, démarrer la location, calculer le montant final au retour |
| Référentiel MVC et API | `CategorieVehicule` — `/CategoriesVehicules` et `/api/categories-vehicules` |

## 1. Prérequis et versions

| Outil | Version utilisée |
|---|---|
| SDK .NET | 10.0.302 |
| ASP.NET Core MVC et Web API | 10.0 |
| Entity Framework Core (SqlServer, Design) | 10.0.10 |
| Outil `dotnet-ef` | 10.0.x (`dotnet tool install --global dotnet-ef`) |
| Base de données | SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) |

Aucun secret n'est versionné : la connexion LocalDB utilise l'authentification Windows.

## 2. Architecture

```text
LocaCar/
├── Domain/          entités, enums, BaseEntity (audit, suppression logique, RowVersion)
├── Application/     IRepository<T>, repositories spécialisés, IUnitOfWork, services, DTO, exceptions métier
├── Infrastructure/  LocaCarDbContext, configurations Fluent API, migrations, Repository<T>, UnitOfWork, seed
├── Web/             ASP.NET Core MVC — CRUD du référentiel CategorieVehicule
├── Api/             Web API REST — CRUD du référentiel + 4 routes métier, fichier Api.http
└── docs/            modèle, diagramme, règles métier, routes, choix et difficultés
```

Sens des références : `Application → Domain`, `Infrastructure → Application + Domain`,
`Web` et `Api → Application + Domain + Infrastructure`. Aucune référence inverse.

Les règles métier R1 à R4 vivent dans `Application/Locations/LocationService.cs`.
Les contrôleurs (MVC et API) n'appellent que des services applicatifs ; ils ne touchent jamais EF Core.
Chaque cas d'utilisation appelle `IUnitOfWork.SaveChangesAsync()` une seule fois : les effets de R3
(contrat créé, véhicule `Loue`, réservation `EnCours`) et de R4 partent dans une même transaction.

## 3. Connexion à la base

La chaîne `ConnectionStrings:DefaultConnection` est présente dans `Api/appsettings.json` et
`Web/appsettings.json` (chacun possède sa propre configuration, les deux visent la même base) :

```text
Server=(localdb)\MSSQLLocalDB;Database=LocaCar;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Pour une autre instance SQL Server, modifier cette valeur dans les deux fichiers.

## 4. Migrations et données de démonstration

La migration initiale se trouve dans `Infrastructure/Persistence/Migrations/`.

Deux façons de créer la base, depuis la racine de la solution :

**Automatique (recommandé pour la démonstration).** Au démarrage en environnement `Development`, `Api` et
`Web` appliquent les migrations puis insèrent le jeu de démonstration si la base est vide
(`Infrastructure/Persistence/DonneesDemonstration.cs`). Il suffit donc de lancer un projet.

**Manuelle.**

```powershell
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj --context LocaCarDbContext
```

Pour repartir d'une base fraîche (identifiants du scénario `.http` garantis) :

```powershell
dotnet ef database drop --force --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj
```

Jeu de démonstration inséré :

| Table | Contenu |
|---|---|
| Agences | 1 `ABJ-PLT` Plateau, 2 `ABJ-COC` Cocody |
| Clients | 1 Kouassi Aya (permis valide jusqu'en 2029), 2 Traore Moussa (permis expirant le 30/09/2026) |
| CategoriesVehicules | 1 `ECO` 25 000 F/jour, pénalité 10 000 ; 2 `SUV` 60 000 / 20 000 ; 3 `LUX` 120 000 / 50 000, sans véhicule |
| Vehicules | ECO : `AA-123-BB`, `AA-456-CC` disponibles, `AA-789-DD` en maintenance ; SUV : `BB-111-EE` |

## 5. Lancement

```powershell
dotnet restore
dotnet build .\LocaCar.slnx
```

Dans deux terminaux séparés :

```powershell
dotnet run --project .\Api\Api.csproj --launch-profile http
```

```powershell
dotnet run --project .\Web\Web.csproj --launch-profile http
```

| Projet | Adresse (profil `http`) | Adresse (profil `https`) |
|---|---|---|
| Api | http://localhost:5124 | https://localhost:7055 |
| Web | http://localhost:5096 | https://localhost:7217 |

Le document OpenAPI est exposé en développement sur `/openapi/v1.json`.

## 6. Routes

### Référentiel — API

| Verbe | Route | Succès | Erreurs |
|---|---|---|---|
| GET | `/api/categories-vehicules` | 200 | — |
| GET | `/api/categories-vehicules/{id}` | 200 | 404 |
| POST | `/api/categories-vehicules` | 201 + `Location` | 400, 409 code déjà utilisé |
| PUT | `/api/categories-vehicules/{id}` | 204 | 400, 404, 409 |
| DELETE | `/api/categories-vehicules/{id}` | 204 (suppression logique) | 404, 409 catégorie encore utilisée |

### Routes métier — API

| Route | Règles | Codes |
|---|---|---|
| `GET /api/vehicules/disponibles?categorieId=&debut=&fin=` | disponibilité sans chevauchement | 200, 400, 404 |
| `POST /api/reservations-locations` | R1, R2, montant estimé (R4.1) | 201, 400, 404, 409 |
| `POST /api/reservations-locations/{id}/demarrer` | R3, une seule unité de travail | 201, 404, 409 |
| `POST /api/contrats/{id}/retour` | R4.2, une seule unité de travail | 200, 400, 404, 409 |
| `GET /api/reservations-locations/{id}`, `GET /api/contrats/{id}` | lectures (cibles des en-têtes `Location`) | 200, 404 |

Les erreurs sont rendues en `ProblemDetails` (RFC 9457). Le type de l'exception métier fixe le code :
`DemandeInvalideException` → 400, `RessourceIntrouvableException` → 404, `ConflitMetierException` → 409.

### Référentiel — écrans MVC

`/CategoriesVehicules` (liste), `/Details/{id}`, `/Create`, `/Edit/{id}`, `/Delete/{id}` (confirmation de
suppression logique). Messages de validation en français, lien « Catégories de véhicules » dans le menu.

## 7. Scénario de démonstration

Le fichier `Api/Api.http` rejoue l'intégralité du scénario, chaque requête étant précédée du résultat
attendu. Il couvre les sept vérifications demandées par le sujet :

| # | Vérification | Requêtes |
|---|---|---|
| 1 | création valide → 201 | A4, C1 |
| 2 | donnée invalide → 400 | A6, C3 à C5, C7, E1 |
| 3 | identifiant inexistant → 404 | A3, A10, C6, D5, E2 |
| 4 | unicité ou règle métier violée → 409 | A7, A9, A11, C9 |
| 5 | workflow métier réussi | C1 → D1 → E3 (réservation, démarrage, retour avec pénalité) |
| 6 | workflow métier refusé | C9 (R2), D4 (transition interdite), E4 (retour déjà enregistré) |
| 7 | suppression logique puis lecture masquée | A12 → A13, puis A14 (code réutilisable) |

Ordre conseillé : base fraîche, lancer `Api`, exécuter les blocs A à F dans l'ordre.
Les dates du scénario (novembre et décembre 2026) doivent rester dans le futur ; les variables en tête
de fichier permettent de les ajuster.

Le scénario a été rejoué intégralement le 10/09/2026 avec un script curl : 44 requêtes, 44 codes conformes.

### Calcul du montant (R4)

```text
joursEstimes  = Ceiling((Fin - Debut).TotalDays)              MontantEstime = joursEstimes x TarifJournalier
joursFactures = Ceiling((RetourReel - Depart).TotalDays)
joursRetard   = RetourReel > RetourPrevu ? Ceiling((RetourReel - RetourPrevu).TotalDays) : 0
MontantFinal  = joursFactures x TarifJournalier + joursRetard x PenaliteRetardParJour
```

Exemple : ECO 25 000 / 10 000, départ 10/08 08h00, retour prévu 12/08 08h00, retour réel 13/08 10h00 →
`Ceiling(3,083) = 4` jours facturés et `Ceiling(1,083) = 2` jours de retard → `100 000 + 20 000 = 120 000 F`.
La réponse de `POST /api/contrats/{id}/retour` renvoie `joursFactures` et `joursRetard` pour vérifier le calcul.

## 8. Limites connues

- `Depart` est l'instant du démarrage effectif, pas le `Debut` de la réservation : si l'on démarre une
  réservation longtemps avant sa date, les jours facturés courent depuis le démarrage.
- Les transitions `EnAttente → Confirmee` et `→ Annulee` ne sont pas exposées par l'API : le démarrage
  accepte `EnAttente` et `Confirmee` (cas prévu par la règle R3 du sujet).
- Pas de gestion des conflits de concurrence `RowVersion` en 409 côté API (la colonne existe et est
  alimentée, le mapping de `DbUpdateConcurrencyException` reste une perspective).
- Pas d'authentification ni de pagination : hors périmètre du sujet.
- Le référentiel MVC couvre uniquement `CategorieVehicule` ; agences, clients et véhicules sont gérés
  par le jeu de démonstration.

## 9. Autres documents

- `docs/Cahier-des-charges.md` — sujet complet du fil rouge (fiche 5.4 : LocaCar)
- `docs/Modele.md`, `docs/modele-diagramme.md` — modèle et diagramme des entités
- `docs/Regles-metier.md` — enums, règles R1 à R4, codes HTTP
- `docs/Routes.md` — routes prévues au jalon 1
- `docs/Choix-et-difficultes.md` — note sur les choix réalisés et les difficultés rencontrées
- `AIDES.md` — déclaration des usages de l'IA et des sources consultées
