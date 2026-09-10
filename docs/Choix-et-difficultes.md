# LocaCar — Choix réalisés et difficultés rencontrées

> Note courte demandée au livrable 7 du sujet.

## 1. Choix de conception

| Choix | Décision | Pourquoi |
|---|---|---|
| Ce que l'on réserve | Une **catégorie**, jamais un véhicule | Au moment de la réservation, seul le tarif compte. Le véhicule physique est affecté au démarrage (R3). C'est ce qui rend la règle R2 nécessaire : compter la capacité, pas les voitures nommées. |
| Périodes | `DateTime` et non `DateOnly` | R4 facture toute journée entamée avec `Math.Ceiling` sur `TotalDays` : il faut la partie horaire. Le permis, lui, est un `DateOnly`. |
| Chevauchement | `debutA < finB && debutB < finA`, bornes exclusives | Convention `[début, fin[` imposée par le sujet : une location qui finit à 08h00 ne bloque pas une location qui commence à 08h00. |
| Retard facturé deux fois | Jours de retard dus au tarif **et** pénalité ajoutée | Le véhicule est réellement immobilisé pendant le retard ; la pénalité est une sanction, pas un remplacement. |
| Caution | Stockée et affichée, jamais dans un calcul | Le sujet ne la fait intervenir dans aucune formule. |
| Codes HTTP | Le **type** de l'exception fixe le code | `DemandeInvalideException` → 400, `RessourceIntrouvableException` → 404, `ConflitMetierException` → 409, mappés une seule fois dans `ExceptionsMetierHandler`. Aucun contrôleur ne décide d'un code d'erreur. |
| Suppression logique | Interceptée dans `SaveChanges` | Un `Remove()` devient `IsDeleted = true`. Aucun service n'a à y penser ; le filtre global masque ensuite la ligne. Les index uniques sont filtrés sur `IsDeleted = 0` pour qu'un code libéré soit réutilisable. |
| Repositories et Unit of Work | `IRepository<T>` + 4 repositories spécialisés + `IUnitOfWork` | Les requêtes spécialisées (véhicules libres, réservations actives chevauchantes, chargements avec `Include`) vivent dans les repositories ; la **décision** métier (comparer les comptages, changer les statuts) reste dans le service. Tous partagent le `DbContext` de la requête, donc un seul `SaveChangesAsync`. |
| Montant estimé | Calculé côté serveur, absent du DTO d'entrée | Un client ne doit jamais pouvoir envoyer son propre prix. |
| Horloge | Abstraction `IHorloge` | R1 (« période future ») et R3/R4 (« départ = maintenant ») dépendent de l'heure courante. L'interface permet de la fixer pendant les vérifications. |
| Web et Api | Même service applicatif | Le contrôleur MVC et le contrôleur API appellent `ICategorieVehiculeService`. Aucune logique n'est dupliquée dans une vue. |

## 2. Difficultés rencontrées

1. **Comprendre pourquoi R2 doit compter et non chercher.** Ma première idée était « il existe un
   véhicule libre → accepté ». Avec deux clients réservant la même unique voiture, les deux auraient été
   acceptés puis un seul aurait pu démarrer. Il fallait comparer `nbVehiculesLibres > nbReservationsActives`.

2. **Faire tenir R3 dans une seule transaction.** Trois écritures (contrat, statut du véhicule, statut de
   la réservation) doivent réussir ou échouer ensemble. La solution est de ne pas appeler `SaveChangesAsync`
   dans les repositories, seulement une fois à la fin du cas d'utilisation, sur l'`IUnitOfWork`.

3. **Distinguer 400 et 409.** Une fin antérieure au début est une demande incohérente (400). Une catégorie
   sans capacité est une demande correcte que l'état du système refuse (409). Cette distinction a guidé le
   choix de trois exceptions distinctes.

4. **La projection EF Core.** Un mapping écrit comme méthode C# force le chargement des entités complètes.
   Écrit comme `Expression<Func<...>>`, il se traduit en SQL (`Projection` dans `CategorieVehiculeService`).

5. **Le filtre global et les navigations.** Une catégorie supprimée logiquement ne doit plus apparaître,
   mais un véhicule qui la référence existe encore. D'où le refus de suppression (409) tant que des véhicules
   ou des réservations en cours y sont rattachés, en plus du `Restrict` sur les clés étrangères.

6. **Le passage aux repositories après coup.** La première version des services utilisait directement
   une interface du `DbContext`. Le sujet exige repositories et Unit of Work : la refonte a consisté à
   déplacer les requêtes dans les repositories sans déplacer les règles, puis à rejouer intégralement le
   scénario `.http` pour vérifier que les codes HTTP n'avaient pas changé.

7. **Culture et formulaires MVC.** Les montants sont saisis en francs CFA avec la culture `fr-FR` ;
   les champs numériques sont des champs texte pour accepter la virgule, et les messages de liaison de modèle
   ont été traduits dans `Web/Program.cs`.
