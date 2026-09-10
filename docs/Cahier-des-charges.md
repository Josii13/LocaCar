# Projets individuels — Réinvestissement du fil rouge .NET

**Contexte :** après le projet fil rouge `GestionHoteliere`, chaque étudiant réalise seul une nouvelle application de gestion. Le domaine change, mais la démarche technique, l'architecture et le niveau de difficulté restent communs.

Ce document contient :

1. le socle obligatoire pour tous ;
2. les huit attributions individuelles ;
3. les jalons de réalisation ;
4. les livrables ;
5. le barème commun sur 20.

Chaque étudiant crée une nouvelle solution correspondant à son sujet. `GestionHoteliere` sert de modèle de démarche et d'architecture ; il ne faut pas copier le dépôt puis remplacer seulement les noms des classes.

Les procédures détaillées déjà étudiées restent disponibles dans [le CRUD MVC](Etape-09-CRUD-TypeChambre.md) et [la création de l'API](Etape-10-API-TypeChambre.md). Elles doivent être adaptées au nouveau domaine et comprises, pas recopiées mécaniquement.

---

## 1. Attribution des sujets

| Étudiant | Projet | Domaine |
|---|---|---|
| Elisee Kambire | **MediPlan** | Centre médical et rendez-vous |
| Cedric Djiré | **PermisPro** | Auto-école et séances de conduite |
| Frank Koffi | **RestauFlow** | Restaurant, réservations et commandes |
| Caleb N'Cho | **LocaCar** | Location de véhicules |
| Samiratou Noufou | **PharmaStock** | Pharmacie, lots et ventes |
| Israel Boka | **BiblioPlus** | Bibliothèque et emprunts |
| Vianney Komenan | **AgriCoop** | Coopérative agricole et collectes |
| Isaac Aka | **IvoireTransit** | Transport interurbain et billetterie |

Les sujets ne sont pas classés par difficulté. Le **socle noté** est identique pour tous : un workflow principal, quatre règles métier, quatre routes métier et une requête de calcul ou de disponibilité. Les approfondissements propres à chaque domaine sont facultatifs.

### Informations à compléter par l'enseignant

| Élément | Valeur |
|---|---|
| Date de démarrage | À compléter |
| Date limite de remise | À compléter |
| Dates de soutenance | À compléter |
| Mode de remise | Lien du dépôt Git à transmettre selon la procédure du cours |
| Nom du dépôt | `nom-prenom-nomduprojet` en minuscules, sans espace ni accent |
| Point de départ | Premier commit portant le tag `depart-projet-individuel` |

---

## 2. Objectifs pédagogiques communs

Chaque étudiant doit démontrer qu'il sait :

- transformer un besoin métier en modèle objet et relationnel ;
- organiser une solution .NET en couches ;
- configurer correctement les références entre projets ;
- utiliser EF Core avec SQL Server et des migrations ;
- implémenter repositories, Unit of Work et service métier ;
- réaliser un CRUD MVC avec validation ;
- exposer une API REST avec DTO et codes HTTP adaptés ;
- appliquer des règles métier sans les placer dans les vues ;
- vérifier les cas de réussite et d'erreur avec des requêtes reproductibles ;
- justifier ses choix pendant une soutenance.

---

## 3. Architecture obligatoire

Chaque solution contient les projets suivants :

```text
NomDuProjet/
├── Domain/
├── Application/
├── Infrastructure/
├── Web/
├── Api/
└── docs/
```

### 3.1 Responsabilités

| Projet | Responsabilité |
|---|---|
| `Domain` | Entités, enums et règles intrinsèques du domaine |
| `Application` | Interfaces de repositories, `IUnitOfWork`, services et contrats applicatifs |
| `Infrastructure` | `DbContext`, EF Core, migrations, repositories et `UnitOfWork` |
| `Web` | Interface ASP.NET Core MVC et vues Razor |
| `Api` | Contrôleurs REST, DTO HTTP et OpenAPI |

### 3.2 Sens des références

```text
Application ------> Domain
Infrastructure ---> Application + Domain
Web --------------> Application + Domain + Infrastructure
Api --------------> Application + Domain + Infrastructure
```

Contraintes :

- `Domain` ne référence aucune autre couche du projet ;
- `Application` ne référence jamais `Infrastructure`, `Web` ou `Api` ;
- un `using` ne remplace jamais une `ProjectReference` ;
- les contrôleurs ne créent jamais eux-mêmes un `DbContext` ou un repository.

---

## 4. Socle technique obligatoire

### 4.1 Création de la solution

Créer un nouveau dossier portant le nom du projet, ouvrir PowerShell dans ce dossier, puis exécuter les commandes suivantes. Remplacer `<NomProjet>` par le nom attribué, par exemple `MediPlan`.

```powershell
dotnet new sln -n <NomProjet> --format slnx
dotnet new classlib -n Domain
dotnet new classlib -n Application
dotnet new classlib -n Infrastructure
dotnet new mvc -n Web
dotnet new webapi -n Api --use-controllers

dotnet sln .\<NomProjet>.slnx add `
  .\Domain\Domain.csproj `
  .\Application\Application.csproj `
  .\Infrastructure\Infrastructure.csproj `
  .\Web\Web.csproj `
  .\Api\Api.csproj
```

Configurer ensuite les références dans le sens imposé :

```powershell
dotnet add .\Application\Application.csproj reference `
  .\Domain\Domain.csproj

dotnet add .\Infrastructure\Infrastructure.csproj reference `
  .\Application\Application.csproj `
  .\Domain\Domain.csproj

dotnet add .\Web\Web.csproj reference `
  .\Application\Application.csproj `
  .\Domain\Domain.csproj `
  .\Infrastructure\Infrastructure.csproj

dotnet add .\Api\Api.csproj reference `
  .\Application\Application.csproj `
  .\Domain\Domain.csproj `
  .\Infrastructure\Infrastructure.csproj
```

Vérifier qu'aucune référence inverse n'a été ajoutée : `Domain` et `Application` ne doivent toujours pas dépendre d'`Infrastructure`, de `Web` ou d'`Api`.

### 4.2 Packages et outils

Toutes les versions EF Core du projet doivent être alignées sur `10.0.10` :

```powershell
dotnet add .\Infrastructure\Infrastructure.csproj package `
  Microsoft.EntityFrameworkCore.SqlServer --version 10.0.10
dotnet add .\Infrastructure\Infrastructure.csproj package `
  Microsoft.EntityFrameworkCore.Design --version 10.0.10

dotnet add .\Web\Web.csproj package `
  Microsoft.EntityFrameworkCore.SqlServer --version 10.0.10

dotnet add .\Api\Api.csproj package `
  Microsoft.EntityFrameworkCore.SqlServer --version 10.0.10
dotnet add .\Api\Api.csproj package `
  Microsoft.EntityFrameworkCore.Design --version 10.0.10
```

Installer l'outil EF s'il est absent :

```powershell
dotnet tool install --global dotnet-ef --version 10.0.10
```

S'il est déjà installé, utiliser plutôt :

```powershell
dotnet tool update --global dotnet-ef --version 10.0.10
```

Puis vérifier le point de départ :

```powershell
dotnet restore
dotnet build .\<NomProjet>.slnx
dotnet list .\Api\Api.csproj reference
```

### 4.3 Technologies

- .NET 10 ;
- ASP.NET Core MVC ;
- ASP.NET Core Web API ;
- Entity Framework Core avec SQL Server ;
- nullable reference types activés ;
- méthodes d'accès aux données asynchrones.

### 4.4 Modèle commun

Chaque sujet comporte **six à huit entités obligatoires utiles**, sans compter une éventuelle classe de base. Toutes doivent participer à une relation et à au moins un cas d'usage demandé. Les extensions facultatives ne font pas partie du socle noté. Toute autre entité nécessite une justification et l'accord de l'enseignant.

Créer une classe `BaseEntity` contenant au minimum :

```csharp
[Key]
public int Id { get; set; }
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
public DateTimeOffset? UpdatedAt { get; set; }
public bool IsDeleted { get; set; }
public DateTimeOffset? DeletedAt { get; set; }

[Timestamp]
public byte[]? RowVersion { get; set; }
```

Le projet doit également comporter :

- au moins deux enums métier ;
- au moins trois relations un-à-plusieurs ;
- au moins une relation représentant une transaction ou une ligne de détail ;
- des attributs de validation et des configurations Fluent API ;
- au moins un index unique pertinent ;
- un filtre global pour la suppression logique ;
- une migration initiale placée dans `Infrastructure`.

Pour un champ unique soumis à la suppression logique, utiliser un index filtré afin qu'une ancienne ligne supprimée ne bloque pas indéfiniment la valeur :

```csharp
entity.HasIndex(x => x.Libelle)
    .IsUnique()
    .HasFilter("[IsDeleted] = 0");
```

Adapter le nom du champ à chaque domaine.

### 4.5 Accès aux données

Chaque projet doit contenir :

- `IRepository<T>` et `Repository<T>` ;
- au moins deux repositories spécialisés ;
- `IUnitOfWork` et `UnitOfWork` ;
- un seul `DbContext` partagé pendant une requête ;
- au moins une requête spécialisée avec `Where`, `Any`, dates ou disponibilité ;
- les méthodes de repository ne sauvegardent pas automatiquement ;
- `SaveChangesAsync()` appelé **une seule fois à la fin d'un cas d'usage réussi**, afin d'enregistrer atomiquement toutes les modifications qui partagent le même `DbContext`.

### 4.6 Service métier

Chaque étudiant implémente au moins un service correspondant au workflow principal indiqué dans sa fiche.

Le service doit :

- recevoir ses dépendances par injection ;
- vérifier les règles de sa fiche qui relèvent du workflow principal ;
- produire un résultat ou une erreur métier compréhensible ;
- ne dépendre ni de MVC, ni de `ControllerBase`, ni des vues Razor ;
- pouvoir être démontré à travers les routes de l'API.

Les quatre règles indiquées dans chaque fiche sont obligatoires. Si l'une d'elles relève d'une autre commande, elle peut être placée dans un second service applicatif léger.

Pour les sujets utilisant des périodes, adopter la convention d'intervalle `[début, fin[` : une opération qui commence exactement à la fin d'une autre est autorisée. La condition de chevauchement est donc :

```csharp
nouveauDebut < finExistante && nouvelleFin > debutExistant
```

### 4.7 Partie MVC

Le référentiel indiqué dans chaque sujet doit avoir :

- une page de liste ;
- une page de détails ;
- une page de création ;
- une page de modification ;
- une confirmation de suppression logique ;
- des messages de validation en français ;
- un lien dans le menu principal.

La suppression du référentiel doit être refusée lorsqu'il est encore utilisé par une donnée métier active.

Le contrôleur MVC utilise `IUnitOfWork` ou un service applicatif. Il ne doit pas utiliser directement EF Core.

### 4.8 Partie API

Le même référentiel doit être exposé avec :

```text
GET    /api/{ressource}
GET    /api/{ressource}/{id}
POST   /api/{ressource}
PUT    /api/{ressource}/{id}
DELETE /api/{ressource}/{id}
```

L'API doit également exposer les opérations métier précisées dans la fiche individuelle.

Contraintes API :

- utiliser des DTO de requête et de réponse ;
- ne jamais sérialiser directement une entité EF Core ;
- retourner `200`, `201`, `204`, `400`, `404` et `409` lorsque la situation l'exige ;
- retourner `201 Created` avec un en-tête `Location` après une création ;
- retourner `409 Conflict` lorsqu'une règle métier, une unicité ou une dépendance interdit l'opération ;
- documenter et vérifier les routes dans un fichier `.http` ;
- exposer le document OpenAPI en développement.

Les ports de développement sont ceux du fichier `Api/Properties/launchSettings.json`. Le fichier `.http` doit reprendre l'adresse réellement affichée par `dotnet run`; recopier les ports de `GestionHoteliere` sans vérifier n'est pas une configuration valide.

### 4.9 Vérifications fonctionnelles

Les tests automatisés n'ayant pas encore été étudiés, ils ne sont **ni obligatoires ni notés** dans ce projet.

Le fichier `.http` doit toutefois permettre de rejouer au minimum les vérifications suivantes :

1. une création valide avec une réponse `201` ;
2. une donnée invalide avec une réponse `400` ;
3. un identifiant inexistant avec une réponse `404` ;
4. une unicité ou une règle métier violée avec une réponse `409` ;
5. un workflow métier réussi ;
6. un workflow métier refusé ;
7. une suppression logique suivie d'une lecture prouvant que la donnée est masquée.

Chaque requête doit être précédée d'un commentaire indiquant le résultat attendu. La démonstration finale doit pouvoir être rejouée dans un ordre clair sur une base préparée.

### 4.10 Limites du périmètre

Sauf mention contraire dans la fiche, les éléments suivants ne sont pas exigés : paiement bancaire réel, SMS, email, génération PDF, QR code, application mobile, SPA, microservices ou service externe.

Ils peuvent être présentés comme perspectives ou bonus uniquement lorsque tout le socle obligatoire est terminé. Une fonctionnalité supplémentaire ne compense pas une règle métier, une vérification ou une couche manquante.

---

## 5. Fiches individuelles

### 5.1 Elisee Kambire — MediPlan

#### Contexte

Un centre médical souhaite organiser les spécialités, les médecins, leurs plages de travail et les rendez-vous des patients.

#### Entités obligatoires

1. `Patient` — numéro patient, nom, prénom, téléphone, date de naissance ;
2. `Specialite` — code, libellé, description ;
3. `Medecin` — matricule, nom, téléphone, spécialité, actif ;
4. `PlageDisponibilite` — médecin, jour de semaine, heure de début, heure de fin ;
5. `RendezVous` — patient, médecin, début, fin, mode, motif, statut ;
6. `Consultation` — rendez-vous, diagnostic, observations, date.

Extensions facultatives : `Medicament`, `Ordonnance` et `LigneOrdonnance`.

#### Enums minimum

- `StatutRendezVous` : `Planifie`, `Realise`, `Annule`, `Absent` ;
- `ModeConsultation` : `Presentiel`, `Teleconsultation`.

#### Référentiel MVC et API

`Specialite` — `/api/specialites`.

#### Workflow principal

`RendezVousService.PlanifierAsync(...)`.

#### Quatre règles obligatoires

1. Le rendez-vous est futur, sa fin est postérieure à son début et sa durée est un multiple de 30 minutes.
2. Le médecin est actif et le rendez-vous se situe dans l'une de ses plages de travail ; à défaut de configuration, utiliser `08:00–17:00`.
3. Le médecin ne peut pas avoir deux rendez-vous qui se chevauchent.
4. Le patient ne peut pas avoir deux rendez-vous qui se chevauchent et un rendez-vous réalisé ne peut plus être annulé.

#### Quatre routes métier

```text
GET   /api/medecins/{id}/creneaux-disponibles?date=...
POST  /api/rendez-vous
PATCH /api/rendez-vous/{id}/annuler
POST  /api/rendez-vous/{id}/terminer
```

#### Calcul ou disponibilité à démontrer

Retourner les créneaux de 30 minutes disponibles pour une journée, dans les plages du médecin et hors rendez-vous existants.

---

### 5.2 Cedric Djiré — PermisPro

#### Contexte

Une auto-école souhaite suivre les inscriptions et planifier des séances de conduite avec un moniteur et un véhicule.

#### Entités obligatoires

1. `Eleve` — numéro, nom, prénom, téléphone, date de naissance ;
2. `CategoriePermis` — code, libellé, nombre minimal d'heures, tarif ;
3. `Inscription` — élève, catégorie, date, statut ;
4. `Moniteur` — matricule, nom, téléphone, actif ;
5. `Vehicule` — immatriculation, modèle, catégorie compatible, statut ;
6. `SeanceConduite` — inscription, moniteur, véhicule, début, fin, statut.

Extensions facultatives : `Examen` et `Paiement`.

#### Enums minimum

- `StatutInscription` : `EnCours`, `Suspendue`, `Terminee`, `Annulee` ;
- `StatutSeance` : `Planifiee`, `Realisee`, `Annulee` ;
- `StatutVehicule` : `Disponible`, `EnSeance`, `Maintenance`.

#### Référentiel MVC et API

`CategoriePermis` — `/api/categories-permis`.

#### Workflow principal

`PlanificationSeanceService.PlanifierAsync(...)`.

#### Quatre règles obligatoires

1. L'inscription est en cours et la séance est placée dans le futur avec une durée positive.
2. L'élève, le moniteur et le véhicule sont libres pendant toute la période.
3. Le moniteur est actif ; le véhicule est disponible et compatible avec la catégorie du permis.
4. Seules les séances réalisées sont additionnées pour déterminer si le minimum d'heures de la catégorie est atteint.

#### Quatre routes métier

```text
POST  /api/seances-conduite
PATCH /api/seances-conduite/{id}/annuler
PATCH /api/seances-conduite/{id}/realiser
GET   /api/inscriptions/{id}/progression
```

#### Calcul ou disponibilité à démontrer

Retourner les heures réalisées, les heures restantes et l'éligibilité à l'examen pratique.

---

### 5.3 Frank Koffi — RestauFlow

#### Contexte

Un restaurant souhaite gérer sa carte et le cycle d'une commande, depuis l'ouverture jusqu'au règlement.

#### Entités obligatoires

1. `Client` — nom, téléphone, email ;
2. `TableRestaurant` — numéro, capacité, zone, active ;
3. `CategoriePlat` — code, libellé, description ;
4. `Plat` — nom, catégorie, prix, disponible ;
5. `Commande` — client éventuel, table, date, statut, montant total ;
6. `LigneCommande` — commande, plat, quantité, prix unitaire ;
7. `Paiement` — commande, date, montant, mode.

Extension facultative : `ReservationTable` et son workflow de réservation.

#### Enums minimum

- `StatutCommande` : `Ouverte`, `EnPreparation`, `Servie`, `Payee`, `Annulee` ;
- `ModePaiement` : `Especes`, `Carte`, `MobileMoney`.

#### Référentiel MVC et API

`CategoriePlat` — `/api/categories-plats`.

#### Workflow principal

`CommandeService.GererAsync(...)`, de l'ouverture au paiement.

#### Quatre règles obligatoires

1. Une ligne possède une quantité strictement positive et concerne un plat disponible.
2. Le serveur copie le prix actuel du plat et recalcule toujours le total depuis les lignes.
3. Une commande payée ou annulée ne peut plus être modifiée.
4. La somme des paiements ne dépasse jamais le total ; la commande devient `Payee` uniquement lorsque les deux montants sont égaux.

#### Quatre routes métier

```text
GET  /api/plats/disponibles?categorieId=...
POST /api/commandes
POST /api/commandes/{id}/lignes
POST /api/commandes/{id}/paiements
```

#### Calcul ou disponibilité à démontrer

Retourner le total recalculé, le montant déjà payé et le reste à payer.

---

### 5.4 Caleb N'Cho — LocaCar

#### Contexte

Une société souhaite réserver un véhicule, démarrer la location et calculer le montant final lors du retour.

#### Entités obligatoires

1. `Agence` — code, nom, ville, adresse ;
2. `Client` — numéro, nom, téléphone, numéro et expiration du permis ;
3. `CategorieVehicule` — code, libellé, tarif journalier, caution, pénalité de retard par jour ;
4. `Vehicule` — immatriculation, modèle, catégorie, agence, statut ;
5. `ReservationLocation` — client, catégorie, début, fin, statut, montant estimé ;
6. `ContratLocation` — réservation, véhicule, départ, retour prévu, retour réel, montant final.

Extensions facultatives : `Entretien` et `Paiement`.

#### Enums minimum

- `StatutVehicule` : `Disponible`, `Loue`, `Maintenance`, `HorsService` ;
- `StatutReservationLocation` : `EnAttente`, `Confirmee`, `EnCours`, `Terminee`, `Annulee`.

#### Référentiel MVC et API

`CategorieVehicule` — `/api/categories-vehicules`.

#### Workflow principal

`LocationService.GererAsync(...)`, de la réservation au retour.

#### Quatre règles obligatoires

1. La période est future, sa fin est postérieure à son début et le permis reste valide jusqu'au retour prévu.
2. Un véhicule disponible de la catégorie demandée doit exister sans chevauchement sur la période.
3. Le démarrage affecte ce véhicule, crée le contrat et fait passer le véhicule à `Loue` dans une seule unité de travail.
4. Le serveur facture tout jour commencé avec `Math.Ceiling`; chaque jour commencé après le retour prévu ajoute la pénalité définie par la catégorie.

#### Quatre routes métier

```text
GET  /api/vehicules/disponibles?categorieId=...&debut=...&fin=...
POST /api/reservations-locations
POST /api/reservations-locations/{id}/demarrer
POST /api/contrats/{id}/retour
```

#### Calcul ou disponibilité à démontrer

Retourner les véhicules disponibles et expliquer le calcul du montant estimé ou final.

---

### 5.5 Samiratou Noufou — PharmaStock

#### Contexte

Une pharmacie souhaite gérer les lots avec date d'expiration et valider des ventes sans utiliser de stock expiré.

#### Entités obligatoires

1. `CategorieProduit` — code, libellé, description ;
2. `Produit` — code, nom, catégorie, prix de vente, seuil d'alerte ;
3. `Fournisseur` — code, raison sociale, téléphone ;
4. `Approvisionnement` — fournisseur, date, numéro de facture ;
5. `LotStock` — produit, approvisionnement, numéro de lot, expiration, quantité initiale, quantité restante ;
6. `Vente` — numéro, date, montant total, statut ;
7. `LigneVente` — vente, lot choisi, quantité, prix unitaire.

Extension facultative : `Paiement`. L'allocation d'une même demande sur plusieurs lots est également facultative.

#### Enums minimum

- `StatutVente` : `Ouverte`, `Validee`, `Annulee` ;
- `StatutApprovisionnement` : `Saisi`, `Receptionne`, `Annule`.

#### Référentiel MVC et API

`CategorieProduit` — `/api/categories-produits`.

#### Workflow principal

`VenteService.ValiderAsync(...)`.

#### Quatre règles obligatoires

1. Le DTO d'ajout reçoit `ProduitId` et `Quantite` ; la quantité est strictement positive et la vente est ouverte.
2. Le serveur choisit le lot non expiré dont l'expiration est la plus proche et qui couvre à lui seul la quantité demandée.
3. Le prix vient du produit, jamais du client, et le total est recalculé depuis les lignes.
4. La validation décrémente les lots et fige la vente dans une seule unité de travail ; un stock insuffisant produit un conflit.

#### Quatre routes métier

```text
GET  /api/produits/{id}/stock-disponible
POST /api/ventes
POST /api/ventes/{id}/lignes
POST /api/ventes/{id}/valider
```

#### Calcul ou disponibilité à démontrer

Retourner le stock total non expiré d'un produit. En approfondissement, répartir automatiquement une quantité sur plusieurs lots selon FEFO.

---

### 5.6 Israel Boka — BiblioPlus

#### Contexte

Une bibliothèque souhaite gérer ses exemplaires physiques, les emprunts, les retours et les pénalités de retard.

#### Entités obligatoires

1. `Adherent` — numéro, nom, téléphone, date d'adhésion, actif ;
2. `CategorieLivre` — code, libellé, durée maximale d'emprunt, pénalité par jour ;
3. `Livre` — ISBN, titre, auteur, catégorie ;
4. `Exemplaire` — code-barres, livre, état, statut ;
5. `Emprunt` — adhérent, exemplaire, dates d'emprunt, d'échéance et de retour ;
6. `Penalite` — adhérent, emprunt, motif, montant, réglée.

Extensions facultatives : `ReservationLivre` et sa file d'attente.

#### Enums minimum

- `StatutExemplaire` : `Disponible`, `Emprunte`, `Perdu`, `Endommage` ;
- `EtatPenalite` : `ARegler`, `Reglee`, `Annulee`.

#### Référentiel MVC et API

`CategorieLivre` — `/api/categories-livres`.

#### Workflow principal

`CirculationService.GererEmpruntAsync(...)`, de l'emprunt au retour.

#### Quatre règles obligatoires

1. L'adhérent est actif, ne possède aucune pénalité non réglée et n'a pas plus de trois emprunts actifs.
2. L'exemplaire est disponible au moment de l'emprunt.
3. Le serveur calcule l'échéance avec la durée maximale de la catégorie.
4. Un retour en retard crée une pénalité égale au nombre de jours commencés multiplié par le tarif journalier de la catégorie ; le retour libère l'exemplaire atomiquement.

#### Quatre routes métier

```text
GET   /api/livres/disponibles?recherche=...
POST  /api/emprunts
POST  /api/emprunts/{id}/retour
PATCH /api/penalites/{id}/regler
```

#### Calcul ou disponibilité à démontrer

Calculer l'échéance et, lors du retour, le montant éventuel de la pénalité. La priorité automatique d'une file de réservation est facultative.

---

### 5.7 Vianney Komenan — AgriCoop

#### Contexte

Une coopérative agricole souhaite enregistrer les lots apportés par ses producteurs et calculer leur valeur selon la qualité.

#### Entités obligatoires

1. `Producteur` — numéro, nom, téléphone, date d'adhésion, actif ;
2. `Parcelle` — producteur, code, localité, superficie ;
3. `ProduitAgricole` — code, libellé, unité de mesure ;
4. `CampagneAgricole` — produit, libellé, dates, prix de base, statut ;
5. `Collecte` — numéro, producteur, campagne, date, montant total ;
6. `LotCollecte` — collecte, parcelle, poids, qualité, taux d'humidité, montant ;
7. `BaremeQualite` — campagne, qualité, taux de prime ou de décote, humidité maximale.

Extensions facultatives : `PaiementProducteur` et une synthèse agrégée de campagne.

#### Enums minimum

- `StatutCampagne` : `Preparee`, `Ouverte`, `Cloturee` ;
- `QualiteProduit` : `Premium`, `Standard`, `Declassee`.

#### Référentiel MVC et API

`ProduitAgricole` — `/api/produits-agricoles`.

#### Workflow principal

`CollecteService.EnregistrerAsync(...)`.

#### Quatre règles obligatoires

1. La campagne est ouverte, la date de collecte appartient à sa période et le producteur est actif.
2. Chaque parcelle appartient au producteur de la collecte et chaque poids est strictement positif.
3. Un lot dont l'humidité dépasse le maximum du barème est refusé.
4. Le serveur calcule `montant = poids × prixDeBase × (1 + tauxDuBareme / 100)` et le total de la collecte est la somme de ses lots.

#### Quatre routes métier

```text
GET  /api/campagnes/ouvertes
POST /api/collectes
POST /api/collectes/{id}/lots
GET  /api/collectes/{id}/valorisation
```

#### Calcul ou disponibilité à démontrer

Retourner, pour chaque lot, la prime ou la décote appliquée et le total calculé. Le taux vient de `BaremeQualite`, pas du client.

---

### 5.8 Isaac Aka — IvoireTransit

#### Contexte

Une compagnie de transport interurbain souhaite planifier des voyages et réserver les sièges de ses bus.

#### Entités obligatoires

1. `Ville` — code, nom, région ;
2. `Trajet` — ville de départ, ville d'arrivée, distance, durée estimée, tarif de base ;
3. `Bus` — immatriculation, modèle, capacité, statut ;
4. `Chauffeur` — matricule, nom, téléphone, numéro de permis, actif ;
5. `Voyage` — trajet, bus, chauffeur, départ prévu, arrivée prévue, statut ;
6. `Passager` — nom, téléphone, pièce d'identité ;
7. `ReservationBillet` — voyage, passager, numéro de siège, date, statut, montant.

Extension facultative : `Paiement`. La gestion de deux réservations réellement simultanées est également facultative.

#### Enums minimum

- `StatutVoyage` : `Planifie`, `Embarquement`, `Parti`, `Arrive`, `Annule` ;
- `StatutReservationBillet` : `Confirmee`, `Annulee`, `Utilisee`.

#### Référentiel MVC et API

`Trajet` — `/api/trajets`.

Les relations `VilleDepartId` et `VilleArriveeId` vers `Ville` doivent être configurées explicitement avec Fluent API et `DeleteBehavior.Restrict`.

#### Workflow principal

`ReservationBilletService.ReserverAsync(...)`.

#### Quatre règles obligatoires

1. Les villes sont différentes ; le voyage est futur, planifié, avec un bus et un chauffeur actifs.
2. Le siège demandé est compris entre `1` et la capacité du bus.
3. Une réservation confirmée bloque le siège ; une réservation annulée le libère et une seconde demande séquentielle reçoit un conflit.
4. Le montant est le tarif du trajet lu par le serveur, jamais une valeur envoyée par le client.

#### Quatre routes métier

```text
GET   /api/voyages?depart=...&arrivee=...&date=...
GET   /api/voyages/{id}/sieges-disponibles
POST  /api/reservations-billets
PATCH /api/reservations-billets/{id}/annuler
```

#### Calcul ou disponibilité à démontrer

Construire la liste exacte des sièges encore disponibles. En approfondissement, ajouter une contrainte d'unicité adaptée et transformer un conflit simultané en réponse `409`.

---

## 6. Jalons communs

### Jalon 1 — Analyse et conception

À présenter avant de coder :

- description du problème ;
- diagramme des entités et cardinalités ;
- liste des enums ;
- règles métier numérotées ;
- routes MVC et API envisagées.

Validation attendue : le périmètre correspond exactement au sujet, sans ajouter prématurément des modules secondaires.

Le diagramme, les cardinalités, les propriétés, les enums et les règles doivent être conçus et validés **avant** la génération des classes métier. Le tag `checkpoint-1` constitue la preuve de ce modèle préalable.

Créer le checkpoint après validation :

```powershell
git tag checkpoint-1
```

### Jalon 2 — Architecture et persistance

- création des cinq projets ;
- références conformes au schéma ;
- entités et configurations EF Core ;
- `DbContext` dans `Infrastructure` ;
- chaîne `ConnectionStrings:<NomDeLaConnexion>` présente dans `Api/appsettings.json` et `Web/appsettings.json` ;
- enregistrement du `DbContext`, du `IUnitOfWork` et des services dans les deux fichiers `Program.cs` ;
- migration initiale ;
- base créée et données de démonstration minimales.

`Api` et `Web` ne lisent pas automatiquement le fichier `appsettings.json` de l'autre projet. Ils peuvent viser la même base, mais chacun doit posséder sa propre configuration de démarrage. Aucun mot de passe réel ne doit être versionné.

Checkpoint :

```powershell
dotnet restore
dotnet build .\<NomProjet>.slnx

dotnet ef migrations add InitialCreate `
  --project .\Infrastructure\Infrastructure.csproj `
  --startup-project .\Api\Api.csproj `
  --context <NomDuDbContext> `
  --output-dir Data\Migrations

dotnet ef migrations list `
  --project .\Infrastructure\Infrastructure.csproj `
  --startup-project .\Api\Api.csproj `
  --context <NomDuDbContext>

dotnet ef database update `
  --project .\Infrastructure\Infrastructure.csproj `
  --startup-project .\Api\Api.csproj `
  --context <NomDuDbContext>
```

Remplacer `<NomDuDbContext>` par le contexte réel du projet. Toutes ces commandes sont exécutées depuis la racine de la solution. Ne pas créer une seconde migration `InitialCreate` si elle existe déjà.

`migrations list` est une vérification facultative et sans écriture : elle confirme que le contexte et les migrations sont trouvés. `database update` est la commande qui applique réellement les migrations à la base.

Ces commandes supposent que `Infrastructure` contient les packages `SqlServer` et `Design`, et que le projet de démarrage `Api` référence directement les couches utilisées ainsi que le package `Design`. Créer ensuite le tag `checkpoint-2`.

### Jalon 3 — Application et règles métier

- repository générique ;
- repositories spécialisés ;
- Unit of Work ;
- service du workflow principal ;
- gestion explicite des réussites et des refus métier.

Checkpoint : relire le service et présenter, dans `docs/Regles-metier.md`, au moins un scénario de réussite et deux scénarios de refus qui seront vérifiés ensuite par l'API.

Créer ensuite le tag `checkpoint-3`.

### Jalon 4 — MVC et API

- CRUD MVC complet du référentiel attribué ;
- suppression logique ;
- DTO et CRUD API ;
- routes métier ;
- fichier `.http` reproductible.

Checkpoint : compiler, lancer les deux projets et démontrer les codes HTTP attendus.

Lancer les projets dans deux terminaux séparés :

```powershell
dotnet run --project .\Web\Web.csproj --launch-profile https
```

```powershell
dotnet run --project .\Api\Api.csproj --launch-profile https
```

Les adresses et les ports sont configurés dans les fichiers `Properties/launchSettings.json`. Créer ensuite le tag `checkpoint-4`.

### Jalon 5 — Finalisation

- README ;
- preuves d'exécution reproductibles ;
- nettoyage du dépôt ;
- préparation de la soutenance.

Vérification finale obligatoire :

```powershell
dotnet build .\<NomProjet>.slnx
git tag checkpoint-5
```

---

## 7. Livrables

Chaque étudiant remet :

1. le dépôt Git complet avec un historique de commits personnel et compréhensible ;
2. un `README.md` donnant l'identité, le sujet, les prérequis, les versions, la connexion, les migrations, les commandes de lancement, les routes, un scénario de démonstration et les limites connues ;
3. le diagramme du modèle dans `docs/` ;
4. les migrations EF Core ;
5. le CRUD MVC demandé ;
6. l'API et son fichier de requêtes `.http` ;
7. une courte note listant les choix réalisés et les difficultés rencontrées.

Ajouter également un fichier `AIDES.md` indiquant les documentations, tutoriels, camarades ou outils d'IA consultés, ce qui a été conservé et comment le résultat a été vérifié.

Format attendu pour `AIDES.md` :

```text
Date | Source ou outil | Demande ou prompt résumé | Fichiers concernés | Élément conservé ou modifié | Vérification effectuée
```

Les preuves principales sont le dépôt exécutable, les migrations et le fichier `.http`. Les captures d'écran peuvent illustrer le README, mais elles ne remplacent pas une démonstration reproductible.

Ne pas remettre :

- les dossiers `bin/` et `obj/` ;
- un mot de passe ou un secret réel ;
- une base locale volumineuse ;
- du code mort ou un second `DbContext` créé accidentellement ;
- un projet qui ne compile que sur la machine de l'étudiant sans procédure documentée.

---

## 8. Soutenance individuelle

Chaque étudiant doit pouvoir expliquer sans lire ses notes :

- le sens de ses `ProjectReference` ;
- la différence entre `AddScoped`, `AddDbContext`, `AddControllers` et `MapControllers` ;
- l'intérêt du DTO ;
- le rôle du repository, du Unit of Work et du service ;
- la différence entre validation de forme et règle métier ;
- les codes `201`, `204`, `400`, `404` et `409` ;
- le fonctionnement de sa suppression logique.

Une petite modification en direct peut porter sur une validation, une route de lecture ou une requête du fichier `.http`. Elle sert à vérifier la maîtrise du projet, pas la vitesse de frappe.

Une fonctionnalité que l'étudiant ne peut ni expliquer ni modifier raisonnablement pendant la soutenance n'est pas considérée comme acquise.

---

## 9. Barème commun sur 20

| Critère | Points | Attendus principaux |
|---|---:|---|
| Modélisation du domaine | 2 | Entités, relations, enums, validations et règles cohérentes |
| Architecture et injection | 2 | Couches, références, responsabilités, DI et absence de dépendance interdite |
| EF Core et persistance | 2 | Configurations, contraintes, migration, suppression logique et audit |
| Repositories, UoW et service métier | 3 | Abstractions utiles, requêtes spécialisées, règles placées au bon niveau |
| CRUD MVC | 2 | Cinq vues, validation, navigation et comportement correct |
| API REST et vérifications | 3 | DTO, routes, codes HTTP, conflits et scénario `.http` reproductible |
| Documentation, Git et jalons | 1,5 | README, AIDES, historique personnel et checkpoints |
| Soutenance individuelle | 4,5 | Module additionnel, Démonstration, explications et petite modification maîtrisée |
| **Total** | **20** | |

### Pénalités transversales possibles

- projet qui ne compile pas : évaluation limitée aux éléments vérifiables ;
- contrôleur contenant directement toute la logique métier : perte de points sur architecture et service ;
- suppression physique à la place de la suppression logique demandée : règle non satisfaite ;
- fonctionnalité non démontrable avec le fichier `.http` : considérée comme non validée.

### Approfondissements facultatifs

Ils peuvent améliorer le critère correspondant, dans la limite de 20/20, mais ne remplacent jamais un élément obligatoire :

- authentification et autorisations cohérentes ;
- pagination et filtrage propres ;
- gestion complète de `RowVersion` avec réponse `409 Conflict` ;
- déploiement documenté ;
- interface particulièrement accessible et soignée.

---

## 10. Règles de travail individuel

### 10.1 Travail personnel

Les étudiants peuvent discuter des concepts communs : architecture, EF Core et HTTP. Le modèle, les règles, les services, les contrôleurs, les DTO et les vérifications remis doivent cependant être compris et réalisés individuellement.

Les huit projets partagent volontairement la même ossature. Une ressemblance dans les noms de couches est donc normale ; une copie de logique métier provenant d'un autre domaine ne l'est pas.

L'historique Git, les scénarios `.http` propres au sujet et la soutenance individuelle font partie de l'évaluation de cette maîtrise.

### 10.2 Usage encadré de l'IA et interdiction du « vibe coding » intégral

Dans ce projet, le « vibe coding » désigne le fait de demander à une IA ou à un agent de construire l'application complète, puis de remettre le résultat sans avoir conçu, écrit, vérifié et compris chaque partie. Cette pratique n'est pas autorisée.

#### Génération de code autorisée

Après validation du jalon 1 et création du tag `checkpoint-1`, l'étudiant peut utiliser une IA pour produire un **premier squelette** des classes d'entités et des enums à partir de son propre modèle validé.

Cette autorisation est limitée à :

- la création des classes déjà présentes dans le diagramme validé ;
- les propriétés, clés étrangères, navigations et enums prévus dans ce modèle ;
- une génération par classe ou par petit groupe cohérent de classes ;
- la correction manuelle par l'étudiant des types, nullabilités, validations et relations obtenus.

Le code généré à cette étape ne doit pas décider de nouvelles entités, de nouvelles relations ou de nouvelles règles à la place de l'étudiant.

#### Usages non autorisés

Il est interdit de demander à une IA ou à un agent de :

- générer ou modifier automatiquement tout le dépôt ;
- réaliser en une seule demande une couche complète ;
- produire à la suite le `DbContext`, les repositories, le Unit of Work, les services, les contrôleurs, les DTO, les vues et les vérifications ;
- implémenter seul les règles métier ou les scénarios `.http` évalués ;
- corriger massivement le projet sans que l'étudiant puisse expliquer la cause des erreurs et les changements retenus.

L'IA peut expliquer une notion, commenter un message d'erreur ou proposer une piste. Pour toutes les parties notées autres que le squelette initial des entités et enums, l'étudiant reste l'auteur de l'implémentation et doit procéder par modifications limitées qu'il comprend et vérifie.

#### Preuves et vérification

Tout usage d'une IA doit être déclaré dans `AIDES.md`. L'étudiant doit y résumer la demande, nommer les fichiers concernés, indiquer ce qu'il a conservé ou corrigé et préciser comment il a vérifié le résultat.

L'historique Git doit permettre de distinguer :

1. le modèle conçu et validé au tag `checkpoint-1` ;
2. le commit contenant les squelettes de classes éventuellement générés ;
3. les commits personnels consacrés à EF Core, aux repositories, aux services, à MVC, à l'API et aux vérifications.

Pendant la soutenance, toute partie non expliquée ou non modifiable raisonnablement peut être considérée comme non acquise, même si elle fonctionne.

---

## 11. Checklist finale de l'étudiant

- [ ] Ma solution contient les cinq projets demandés
- [ ] Mes références suivent le sens imposé
- [ ] Mon modèle a été validé et tagué avant toute génération de classes
- [ ] J'ai toutes les entités obligatoires de mon sujet, sans extension injustifiée
- [ ] Je peux expliquer et modifier les classes dont le squelette a été généré
- [ ] Mes relations et contraintes sont configurées
- [ ] Ma migration initiale peut créer la base
- [ ] Mon Unit of Work partage un seul `DbContext`
- [ ] Mon service vérifie les règles métier obligatoires
- [ ] Mon CRUD MVC du référentiel fonctionne
- [ ] Mon API utilise des DTO
- [ ] Mes réponses HTTP utilisent les bons codes
- [ ] Ma suppression est logique
- [ ] Mon fichier `.http` permet de rejouer la démonstration
- [ ] Tous mes usages d'une IA sont déclarés dans `AIDES.md`
- [ ] Mon README permet à une autre personne de lancer le projet
- [ ] Aucun secret, `bin/` ou `obj/` n'est versionné
- [ ] Je sais expliquer chaque élément pendant la soutenance
