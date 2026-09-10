# LocaCar — Routes prévues

> Document de conception — Jalon 1. Aucune de ces routes n'est encore implémentée.

## 1. Routes métier (API)

### 1.1 Véhicules disponibles

```
GET /api/vehicules/disponibles?categorieId={int}&debut={ISO}&fin={ISO}
```

| Élément | Valeur |
|---|---|
| Rôle | Lister les véhicules affectables sur la période (UC1) |
| Règle appliquée | Recherche de disponibilité (base de R2) |
| Réponse | Liste de `VehiculeDisponibleDto` |
| 200 | Liste, éventuellement vide |
| 400 | `debut >= fin`, dates absentes ou illisibles |
| 404 | `categorieId` inconnu |

Un véhicule est retourné s'il appartient à la catégorie, si son `Statut` est `Disponible`,
et s'il n'a aucun `ContratLocation` chevauchant la période.

### 1.2 Créer une réservation

```
POST /api/reservations-locations
```

Corps : `clientId`, `categorieVehiculeId`, `debut`, `fin`.
`MontantEstime` n'est **jamais** envoyé par le client : il est calculé par le serveur.

| Code | Cas |
|---|---|
| 201 | Créée — en-tête `Location: /api/reservations-locations/{id}` |
| 400 | Champs manquants, ou R1 violée (période passée, fin <= début, permis expiré) |
| 404 | `clientId` ou `categorieVehiculeId` inconnu |
| 409 | R2 violée — plus de capacité sur cette catégorie et cette période |

### 1.3 Démarrer une location

```
POST /api/reservations-locations/{id}/demarrer
```

Applique R3 dans une seule unité de travail.

| Code | Cas |
|---|---|
| 201 | Contrat créé — `Location: /api/contrats/{contratId}` |
| 404 | Réservation inexistante |
| 409 | Statut incompatible (déjà `EnCours`, `Terminee` ou `Annulee`), ou aucun véhicule libre |

### 1.4 Enregistrer le retour

```
POST /api/contrats/{id}/retour
```

Corps : `retourReel` (optionnel ; à défaut, l'instant courant).
Applique R4 dans une seule unité de travail.

| Code | Cas |
|---|---|
| 200 | Retour enregistré — retourne le contrat avec `MontantFinal` |
| 400 | `retourReel` antérieur au départ |
| 404 | Contrat inexistant |
| 409 | Retour déjà enregistré (`RetourReel` non null) |

---

## 2. Référentiel CategorieVehicule — API CRUD

| Verbe | Route | Succès | Erreurs |
|---|---|---|---|
| GET | `/api/categories-vehicules` | 200 | — |
| GET | `/api/categories-vehicules/{id}` | 200 | 404 |
| POST | `/api/categories-vehicules` | 201 + `Location` | 400, 409 (code déjà utilisé) |
| PUT | `/api/categories-vehicules/{id}` | 204 | 400, 404, 409 (code déjà utilisé) |
| DELETE | `/api/categories-vehicules/{id}` | 204 | 404, 409 (référentiel encore utilisé) |

`DELETE` réalise une **suppression logique** (`IsDeleted = true`, `DeletedAt` renseigné).
Elle est refusée en `409` si des véhicules ou des réservations actives référencent encore la
catégorie. Après suppression, un `GET` sur la liste ou sur l'identifiant ne la retourne plus,
grâce au filtre global.

Les entités EF Core ne sont jamais sérialisées : l'API expose des DTO dédiés.

---

## 3. Référentiel CategorieVehicule — écrans MVC

| Action | Route | Écran |
|---|---|---|
| Index | `/CategoriesVehicules` | Liste |
| Details | `/CategoriesVehicules/Details/{id}` | Détail |
| Create | `/CategoriesVehicules/Create` | Formulaire de création (GET + POST) |
| Edit | `/CategoriesVehicules/Edit/{id}` | Formulaire de modification (GET + POST) |
| Delete | `/CategoriesVehicules/Delete/{id}` | Confirmation de suppression logique (GET + POST) |

Contraintes :

- messages de validation **en français** ;
- lien « Catégories de véhicules » dans le menu principal (`_Layout.cshtml`) ;
- le contrôleur MVC **n'accède jamais directement à EF Core** : il passe par le service /
  les repositories ;
- la suppression est logique, et refusée avec un message explicite si la catégorie est
  encore utilisée.

---

## 4. Correspondance routes / règles métier

| Route | R1 | R2 | R3 | R4 |
|---|:--:|:--:|:--:|:--:|
| `GET /api/vehicules/disponibles` | | partiel | | |
| `POST /api/reservations-locations` | X | X | | X (estimé) |
| `POST /api/reservations-locations/{id}/demarrer` | | | X | |
| `POST /api/contrats/{id}/retour` | | | | X (final) |
