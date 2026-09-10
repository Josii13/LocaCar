# LocaCar — Modèle du domaine

> Document de conception — Jalon 1. Rédigé avant toute écriture de classe C#.

## 1. Problème

Une société de location souhaite :

1. **réserver** un véhicule d'une certaine catégorie sur une période donnée ;
2. **démarrer** la location en affectant un véhicule physique et en établissant un contrat ;
3. **calculer le montant final** lorsque le véhicule est rendu, en tenant compte d'un éventuel retard.

## 2. Acteurs

| Acteur | Rôle |
|---|---|
| Client | Demande une réservation, prend et rend le véhicule |
| Agent de comptoir | Confirme la réservation, démarre la location, enregistre le retour |
| Gestionnaire du référentiel | Administre les catégories de véhicules (tarifs, caution, pénalité) |

## 3. Cas d'utilisation principaux

| Code | Cas d'utilisation | Acteur |
|---|---|---|
| UC1 | Consulter les véhicules disponibles pour une catégorie et une période | Agent |
| UC2 | Créer une réservation | Agent |
| UC3 | Démarrer une location (affectation du véhicule + contrat) | Agent |
| UC4 | Enregistrer le retour et calculer le montant final | Agent |
| UC5 | Gérer le référentiel `CategorieVehicule` (CRUD) | Gestionnaire |

## 4. Principe de modélisation central

> **On réserve une CATÉGORIE, on contractualise un VÉHICULE.**

`ReservationLocation` ne référence **aucun** véhicule : au moment de la réservation, seule la
catégorie est connue. C'est le **démarrage** (règle R3) qui affecte un véhicule précis et crée
le `ContratLocation`.

Conséquences directes :

- `MontantEstime` appartient à la réservation (calculé sans connaître le véhicule) ;
- `MontantFinal` appartient au contrat (calculé après le retour réel) ;
- la vérification de disponibilité porte sur une **capacité de catégorie**, pas sur un véhicule
  nommé (voir R2 dans `Regles-metier.md`).

## 5. Classe de base

Toutes les entités héritent de `BaseEntity`.

| Propriété | Type | Rôle |
|---|---|---|
| `Id` | `int` | Clé primaire |
| `CreatedAt` | `DateTime` | Audit — date de création |
| `UpdatedAt` | `DateTime?` | Audit — dernière modification (null si jamais modifié) |
| `IsDeleted` | `bool` | Suppression logique |
| `DeletedAt` | `DateTime?` | Date de suppression logique |
| `RowVersion` | `byte[]` | Concurrence optimiste (`rowversion` SQL Server) |

- La suppression logique est appliquée par un **filtre global** (`HasQueryFilter`) sur
  `IsDeleted == false`.
- Tout index unique sur une donnée métier est **filtré** sur `IsDeleted = 0` (voir §8).

## 6. Entités et attributs

### 6.1 Agence

| Attribut | Type | Contrainte |
|---|---|---|
| `Code` | `string` | requis, max 10, **unique filtré** |
| `Nom` | `string` | requis, max 100 |
| `Ville` | `string` | requis, max 60 |
| `Adresse` | `string` | requis, max 200 |

Navigation : `ICollection<Vehicule> Vehicules`

### 6.2 Client

| Attribut | Type | Contrainte |
|---|---|---|
| `Numero` | `string` | requis, max 20, **unique filtré** |
| `Nom` | `string` | requis, max 100 |
| `Telephone` | `string` | requis, max 20 |
| `NumeroPermis` | `string` | requis, max 30 |
| `ExpirationPermis` | `DateOnly` | requis |

`ExpirationPermis` est une `DateOnly` : c'est une date administrative, pas un instant.
Elle est comparée au `RetourPrevu` dans la règle R1.

Navigation : `ICollection<ReservationLocation> Reservations`

### 6.3 CategorieVehicule — référentiel tarifaire

| Attribut | Type | Contrainte |
|---|---|---|
| `Code` | `string` | requis, max 10, **unique filtré** |
| `Libelle` | `string` | requis, max 100 |
| `TarifJournalier` | `decimal(18,2)` | requis, > 0 |
| `Caution` | `decimal(18,2)` | requis, >= 0 |
| `PenaliteRetardParJour` | `decimal(18,2)` | requis, >= 0 |

`Caution` est **stockée et affichée uniquement** : elle n'entre dans aucun calcul (décision D4).

Navigations : `ICollection<Vehicule> Vehicules`, `ICollection<ReservationLocation> Reservations`

### 6.4 Vehicule

| Attribut | Type | Contrainte |
|---|---|---|
| `Immatriculation` | `string` | requis, max 20, **unique filtré** |
| `Modele` | `string` | requis, max 100 |
| `Statut` | `StatutVehicule` | requis, défaut `Disponible` |
| `CategorieVehiculeId` | `int` | FK requise |
| `AgenceId` | `int` | FK requise |

Navigations : `CategorieVehicule Categorie`, `Agence Agence`, `ICollection<ContratLocation> Contrats`

### 6.5 ReservationLocation

| Attribut | Type | Contrainte |
|---|---|---|
| `ClientId` | `int` | FK requise |
| `CategorieVehiculeId` | `int` | FK requise |
| `Debut` | `DateTime` | requis |
| `Fin` | `DateTime` | requis, strictement > `Debut` |
| `Statut` | `StatutReservationLocation` | requis |
| `MontantEstime` | `decimal(18,2)` | calculé côté serveur, jamais reçu du client |

Navigations : `Client Client`, `CategorieVehicule Categorie`, `ContratLocation? Contrat`

### 6.6 ContratLocation

| Attribut | Type | Contrainte |
|---|---|---|
| `ReservationLocationId` | `int` | FK requise, **unique filtrée** |
| `VehiculeId` | `int` | FK requise |
| `Depart` | `DateTime` | requis |
| `RetourPrevu` | `DateTime` | requis |
| `RetourReel` | `DateTime?` | **null tant que le véhicule n'est pas rendu** |
| `MontantFinal` | `decimal?` | **null tant que le retour n'a pas eu lieu** |

Nullabilités à savoir justifier en soutenance :
`RetourReel` et `MontantFinal` ne valent pas `0` avant le retour — ils **n'existent pas encore**.
Un `0` signifierait « rendu, gratuitement », ce qui est une information fausse.

Navigations : `ReservationLocation Reservation`, `Vehicule Vehicule`

## 7. Relations et cardinalités

| # | Relation | Cardinalité | FK portée par | Suppression |
|---|---|---|---|---|
| A | Agence → Vehicule | 1 — * | `Vehicule.AgenceId` | Restrict |
| B | CategorieVehicule → Vehicule | 1 — * | `Vehicule.CategorieVehiculeId` | Restrict |
| C | Client → ReservationLocation | 1 — * | `ReservationLocation.ClientId` | Restrict |
| D | CategorieVehicule → ReservationLocation | 1 — * | `ReservationLocation.CategorieVehiculeId` | Restrict |
| E | ReservationLocation → ContratLocation | 1 — 0..1 | `ContratLocation.ReservationLocationId` (unique) | Restrict |
| F | Vehicule → ContratLocation | 1 — * | `ContratLocation.VehiculeId` | Restrict |

**Relation transaction / détail** : **E**. Le contrat est la matérialisation transactionnelle de
la réservation ; il n'existe qu'une fois la location démarrée, et au plus une fois par réservation.

**Pourquoi `Restrict` partout** : la suppression est **logique**. Un `Cascade` en base
contredirait ce choix et détruirait des contrats, qui sont des pièces justificatives.
`Restrict` est également ce qui justifie le **409 Conflict** lorsqu'on tente de supprimer une
catégorie encore utilisée.

## 8. Index uniques (tous filtrés sur `IsDeleted = 0`)

| Entité | Colonne(s) | Justification |
|---|---|---|
| Agence | `Code` | identifiant métier |
| Client | `Numero` | identifiant métier |
| CategorieVehicule | `Code` | identifiant métier — support du test 409 |
| Vehicule | `Immatriculation` | unicité légale |
| ContratLocation | `ReservationLocationId` | c'est ce qui impose le 1 — 0..1 |

**Pourquoi filtrés** : sans le filtre, supprimer logiquement la catégorie `ECO` interdirait
définitivement d'en recréer une portant le même code — la ligne supprimée continuerait
d'occuper la valeur dans l'index unique.

## 9. Décisions de conception validées

| # | Décision | Justification |
|---|---|---|
| D1 | `DateTime` pour les périodes ; `DateOnly` pour `ExpirationPermis` | `Math.Ceiling` n'a de sens que si les durées ont une partie fractionnaire |
| D2 | Disponibilité par **capacité de catégorie** | Sans elle, R2 ne protège d'aucune surréservation |
| D3 | Jours réels facturés **+** pénalité sur les jours de retard | Le véhicule est réellement immobilisé ; la pénalité s'ajoute, elle ne remplace pas |
| D4 | `Caution` stockée, jamais calculée | La gérer relèverait d'un module Paiement, hors périmètre |
| D5 | Pas d'`Agence` sur la réservation | Non prévu par le sujet ; l'agence se déduit du véhicule affecté |
| D6 | Deux enums seulement | L'état du contrat se lit dans `RetourReel` (null = en cours) et le statut de la réservation |
| D7 | `decimal` pour tous les montants | `double` introduit des erreurs d'arrondi inacceptables en facturation |

## 10. Conformité au cahier des charges

| Exigence | État |
|---|---|
| 6 entités obligatoires | OK — Agence, Client, CategorieVehicule, Vehicule, ReservationLocation, ContratLocation |
| Au moins 2 enums métier | OK — 2 (voir `Regles-metier.md`) |
| Au moins 3 relations un-à-plusieurs | OK — 5 (A, B, C, D, F) |
| Au moins une relation transaction/détail | OK — E |
| Au moins un index unique pertinent | OK — 5 |
| Filtre global de suppression logique | OK — `IsDeleted == false` |
| Concurrence optimiste | OK — `RowVersion` |
