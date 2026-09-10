# LocaCar — Enums et règles métier

> Document de conception — Jalon 1.

## 1. Enums métier

### 1.1 StatutVehicule

| Valeur | Signification |
|---|---|
| `Disponible` | Le véhicule peut être affecté à une location |
| `Loue` | Le véhicule est actuellement sorti sur un contrat |
| `Maintenance` | Immobilisé temporairement, non affectable |
| `HorsService` | Retiré du parc, non affectable |

### 1.2 StatutReservationLocation

| Valeur | Signification | Transition suivante possible |
|---|---|---|
| `EnAttente` | Créée, non encore confirmée | `Confirmee`, `Annulee` |
| `Confirmee` | Validée, en attente du démarrage | `EnCours`, `Annulee` |
| `EnCours` | Location démarrée, contrat créé | `Terminee` |
| `Terminee` | Véhicule rendu, montant final calculé | — (état terminal) |
| `Annulee` | Abandonnée avant démarrage | — (état terminal) |

Réservations dites **actives** (celles qui occupent de la capacité) : `EnAttente` et `Confirmee`.

Aucun enum de statut pour le contrat : son état se lit dans `RetourReel`
(`null` = en cours, renseigné = clos) et dans le statut de la réservation liée.

---

## 2. Notion transverse : le chevauchement de périodes

Test unique, réutilisé par R2 et par la recherche de disponibilité :

```
[debutA, finA] chevauche [debutB, finB]   <=>   debutA < finB  ET  debutB < finA
```

Les bornes sont **exclusives** : une location qui se termine le 12/08 à 08h00 ne chevauche pas
une location qui commence le 12/08 à 08h00.

---

## 3. Règle R1 — Validité de la période et du permis

**Problème.** On ne doit pas pouvoir réserver dans le passé, ni sur une période incohérente, ni
avec un permis qui expirera pendant la location.

**Conditions (les trois doivent être vraies).**

1. `Debut > maintenant` — la période est future ;
2. `Fin > Debut` — la fin est postérieure au début ;
3. `Client.ExpirationPermis >= date(Fin)` — le permis reste valide jusqu'au retour prévu.

**Comportement attendu.** Si une condition échoue, la réservation n'est **pas** créée et la cause
précise est retournée.

**Cas valide.**
Nous sommes le 29/08/2026. Debut = 05/09/2026 08h00, Fin = 07/09/2026 08h00,
permis expirant le 31/12/2027 → accepté.

**Cas invalides.**

| Données | Cause |
|---|---|
| Debut = 20/08/2026 (passé) | période non future |
| Debut = 07/09, Fin = 05/09 | fin antérieure au début |
| Fin = 07/09/2026, permis expirant le 01/09/2026 | permis expiré avant le retour prévu |

**Résultat métier.** Refus de création.

**Code HTTP.** `400 Bad Request` — la demande elle-même est incohérente ou irrecevable.

---

## 4. Règle R2 — Disponibilité sans chevauchement

**Problème.** Accepter une réservation qu'on ne pourra pas honorer.

**Condition.** Sur la période demandée, la catégorie doit disposer d'au moins une unité libre :

```
nbVehiculesLibres  =  nombre de Vehicule de la catégorie
                      dont Statut = Disponible
                      et sans ContratLocation chevauchant la période

nbReservationsActives = nombre de ReservationLocation de la même catégorie,
                        de statut EnAttente ou Confirmee,
                        chevauchant la période

Condition :  nbVehiculesLibres > nbReservationsActives
```

**Pourquoi ce comptage.** Une réservation `Confirmee` n'a pas encore de véhicule affecté
(l'affectation est faite par R3). Elle réserve donc une **unité de capacité** dans sa catégorie,
et non un véhicule nommé. Sans ce comptage, N clients pourraient réserver la même unique voiture.

**Comportement attendu.** Si la condition est fausse, la réservation n'est pas créée.

**Cas valide.** Catégorie ECO : 3 véhicules `Disponible` sans contrat chevauchant,
1 réservation confirmée sur la période. `3 > 1` → accepté.

**Cas invalide.** Catégorie ECO : 1 véhicule libre, 1 réservation confirmée chevauchante.
`1 > 1` est faux → refusé.

**Résultat métier.** Refus : plus de capacité sur cette catégorie et cette période.

**Code HTTP.** `409 Conflict` — la demande est bien formée, c'est **l'état du système** qui
l'empêche. À distinguer soigneusement du 400 de R1.

---

## 5. Règle R3 — Démarrage atomique de la location

**Problème.** Le démarrage modifie trois choses à la fois. Si l'une échoue et pas les autres,
la base devient incohérente : un véhicule marqué `Loue` sans contrat, ou un contrat sans
véhicule immobilisé.

**Conditions préalables.**

1. La réservation existe et son statut est `Confirmee` (ou `EnAttente`, selon le flux retenu) ;
2. Un véhicule de la catégorie, de statut `Disponible`, sans contrat chevauchant, est trouvé.

**Comportement attendu — une seule unité de travail.**

| Action | Effet |
|---|---|
| 1 | Sélection d'un véhicule libre de la catégorie |
| 2 | Création du `ContratLocation` (`Depart`, `RetourPrevu`, `VehiculeId`, `ReservationLocationId`) |
| 3 | `Vehicule.Statut = Loue` |
| 4 | `Reservation.Statut = EnCours` |

Ces quatre effets sont écrits par **un unique appel à `SaveChangesAsync()`**, à la fin du cas
d'utilisation. EF Core les envoie dans une même transaction : soit tout est appliqué, soit rien.

**Cas valide.** Réservation `Confirmee` sur ECO, véhicule `AA-123-BB` disponible → contrat créé,
véhicule `Loue`, réservation `EnCours`.

**Cas invalides.**

| Situation | Résultat | HTTP |
|---|---|---|
| Réservation inexistante | non trouvée | `404` |
| Réservation déjà `EnCours` ou `Terminee` | transition interdite | `409` |
| Aucun véhicule libre au moment du démarrage | démarrage impossible | `409` |

---

## 6. Règle R4 — Calcul du montant

**Problème.** Facturer tout jour commencé et sanctionner le retard.

### 6.1 Montant estimé (à la réservation)

```
joursEstimes  = Math.Ceiling((Fin - Debut).TotalDays)
MontantEstime = joursEstimes * Categorie.TarifJournalier
```

### 6.2 Montant final (au retour)

```
joursFactures = Math.Ceiling((RetourReel - Depart).TotalDays)

joursRetard   = RetourReel > RetourPrevu
                ? Math.Ceiling((RetourReel - RetourPrevu).TotalDays)
                : 0

MontantFinal  = joursFactures * Categorie.TarifJournalier
              + joursRetard   * Categorie.PenaliteRetardParJour
```

**Pourquoi `Math.Ceiling`.** Toute journée entamée est due. `TotalDays` renvoie un `double`
fractionnaire (2,25 jours) ; `Ceiling` arrondit au jour supérieur (3). C'est précisément pour
cela que les périodes sont modélisées en `DateTime` et non en `DateOnly`.

**Pourquoi le retard est facturé deux fois** (tarif **et** pénalité) : le véhicule reste
réellement immobilisé pendant le retard, donc les jours sont dus ; la pénalité s'y **ajoute**
en tant que sanction. Décision D3 du modèle.

**Exemple chiffré de référence.**

```
Catégorie ECO : tarif 25 000 F/jour, pénalité 10 000 F/jour
Départ       : 10/08 08h00
Retour prévu : 12/08 08h00
Retour réel  : 13/08 10h00

joursFactures = Ceiling(3,0833) = 4   ->  4 x 25 000 = 100 000
joursRetard   = Ceiling(1,0833) = 2   ->  2 x 10 000 =  20 000
                                          MontantFinal = 120 000 F
```

**Effets du retour, dans une seule unité de travail.**

| Action | Effet |
|---|---|
| 1 | `Contrat.RetourReel` renseigné |
| 2 | `Contrat.MontantFinal` calculé |
| 3 | `Vehicule.Statut = Disponible` |
| 4 | `Reservation.Statut = Terminee` |

**Cas invalides.**

| Situation | Résultat | HTTP |
|---|---|---|
| Contrat inexistant | non trouvé | `404` |
| Contrat déjà clos (`RetourReel` non null) | retour déjà enregistré | `409` |
| `RetourReel < Depart` | date incohérente | `400` |

---

## 7. Où vivent ces règles

| Règle | Emplacement | Interdit |
|---|---|---|
| R1, R2, R3, R4 | `Application` — `LocationService` | Jamais dans un contrôleur, jamais dans une vue Razor |
| Validation de forme (champ requis, longueur, format) | DTO / ViewModel | Ne remplace pas une règle métier |

**Distinction à maîtriser pour la soutenance.**
La *validation de forme* répond à « la requête est-elle bien remplie ? » → `400`.
La *règle métier* répond à « l'état du système permet-il cette opération ? » → `409`.

---

## 8. Synthèse des codes HTTP

| Situation | Code |
|---|---|
| Lecture réussie | `200` |
| Création réussie | `201` + en-tête `Location` |
| Modification / suppression réussie sans corps | `204` |
| Requête mal formée, R1 violée, date incohérente | `400` |
| Identifiant inexistant | `404` |
| R2 violée, transition d'état interdite, unicité violée, référentiel encore utilisé | `409` |
