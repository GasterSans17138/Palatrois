# Palatrois – README

**Auteurs** : Ce projet a été fait par 3 programmeurs : Eliot Nerrand, François Lahalle et Marius Piris.

## Architecture Overview

- **Utility System**  
  Système principal d’évaluation des objectifs, basé sur un score de "utility" pour chaque but (exploration, attaque, capture, production…).

- **UtilityGoalDefinition / AttackZoneGoalDefinition**
  - ScriptableObject qui définit les paramètres, courbes, poids pour chaque goal.
  - AttackZoneGoalDefinition gère le choix dynamique de la meilleure cible d’attaque.

- **UtilityContext**
  - Contient l’état perçu du monde (PerceivedWorldState) et les fonctions de récupération des variables de contexte pour le calcul de score.

- **PerceivedWorldState / LocalWorldState**
  - Représentation locale de la map, unités, clusters, tours, usines, zones inexplorées, fog of war, etc.
  - Fonctions utilitaires (ex : EvaluateThreatAround, clustering, etc.)

- **UnitController / BaseEntity**
  - Gestion des entités (unités, usines, tours…) : état, team, influence sur la map, vie, visibilité, influence map, sélection, mort, etc.

- **InfluenceMap / InfluenceNode**
  - Système de propagation d’influences (alliés/ennemis), mémoire des menaces même hors vision directe.
  - Nodes avec position, voisinage, status (exploré/inexploré), accès navmesh…

- **FogOfWarSystem / FogOfWarManager**
  - Gestion de la vision (directe/passée/inexplorée), update du fog et de la visibilité des entités.

- **IInfluencer**
  - Interface pour tout objet générant de l’influence sur la map (unités, usines…).

- **FSM (Finite State Machine)**
  - Contrôle l’état global de chaque agent (MoveTo, Attack, Repair).
  - Change d’état selon l’avancement des plans, événements de jeu, triggers (ex : ennemis rencontrés).
  - Fonctionne avec des ScriptableObject pour chaque Action, Transition et Condition.
  - Remplir les conditions pour chaque transition entre chaque action.

- **GOAP (Goal Oriented Action Planning)**
  - Planification d’actions pour atteindre un but donné (exploration, attaque, etc.).
  - Génère dynamiquement une séquence d’actions, adaptative selon l’état local (NodeGOAP, GroupPlanExecutor, GOAPActions).
  - Replanification si action échoue où le contexte change.
  - Fonctionne avec le même système de ScriptableObject pour chaque Action du GOAP.
  - Précondition et effects à ajouter pour chaque action.

- **Squad / Formation Management**
  - **Upgradable Factory**
    - Création d'un scriptable object pour ajouter les cas d'amélioration
    - UI pour l'amélioration de la factory pour le joueur
  - **Leader in Unit.cs**
    - Création d'un Leader avec le bâtiment principal
    - Activation et désactivation du leader lorsqu'il entre ou sort d'une squad
  - **Squad.cs / SquadManager.cs**
    - Création/groupement d’unités en escouades (squad) avec un Leader pour exécuter un plan commun.
    - Affectation des unités selon les besoins du plan, cohésion spatiale, passage en formation.
    - Déformation d'une squad avec l'UI du joueur ou bien par l'AI Controller
    - Fusion des squads entre elles pour former une plus grosse squad
  - **Formation.cs**
    - Logique de placement (cercle, cercle de protection, spiral, carré, triangle…) selon le rôle et le contexte.
    - Sélection automatique de la formation la plus adaptée à la mission.
    - Changement de la formation possible

## Fonctionnement global

- **1. Gathering**  
  L’IA collecte les données sur l’environnement (unités, clusters, bâtiments, zones, visibilité).

- **2. Utility Evaluation**  
  Chaque goal calcule sa "utility" selon des paramètres contextuels dynamiques (via UtilityContext).

- **3. Dynamic Target Selection**  
  Certains goals (attaque, exploration…) cherchent dynamiquement la meilleure cible dans le monde courant (clusters ennemis, usines, zones à explorer, etc.)

- **4. Assignment**  
  Le goal ayant le meilleur score reçoit la priorité : sélection des unités/factories pour l’action, en tenant compte des ressources et de l’état global.

- **5. Action Execution**
  - FSM choisit le comportement micro (ex : Combat, Repare, Capture...)
  - GOAP génère et exécute une séquence d’actions adaptées au contexte du squad.
  - Execution par squad/groupes d’unités synchronisés, formations adaptées.

- **6. Adaptation/Feedback**  
  Réévaluation continue selon les changements de l’environnement (nouvelles unités, découvertes, menaces, pertes, etc.)

## Points de conception

- **Flexible / Modulaire**  
  - Ajout/suppression/modif de goals, d’actions ou de formations sans toucher au core.
  - Curves et weights éditables dans l’éditeur.
  - Séparation stricte de la data, du calcul, et de l’exécution.

- **Scalable**  
  - Clusterisation dynamique pour optimiser la prise de décision sur les grandes maps.
  - Planification groupée par squads.

- **Robuste**  
  - Fallbacks sur certains goals (ex : production d’unités si blocage, replanification GOAP en cas d’échec).
  - Gestion mémoire de l’influence et du fog.

## Sources & Inspirations

- **Félix Becaud**
  - Merci à Félix Becaud pour ses précieux conseils.
  
- **Utility Theory & Infinite Axis Utility System (IAUS)**
  - Documentation, articles, et GDC talks sur les systèmes d’utility pour l’IA de jeu vidéo.
  - IAUS : référence pour un système modulaire, scalable et décentralisé de décision IA (cf. Infinite Axis Utility System de Dave Mark).
    - [IAUS GitHub Example](https://github.com/DreamersIncStudios/ECS-IAUS-sytstem/tree/Stable-0.8.7-Unity6/Assets/Systems/IAUS)
    - [GameAIPro - Utility Decisions in Behavior Trees (PDF)](https://www.gameaipro.com/GameAIPro/GameAIPro_Chapter10_Building_Utility_Decisions_into_Your_Existing_Behavior_Tree.pdf)

- **Concepts clés abordés**
  - Moyenne géométrique (Geometric Mean) pour mixer plusieurs facteurs de scoring.
  - Théorie de l’utilité (Utility Theory) appliquée à la prise de décision adaptative.
  - Exemples d’IA MMO : gestion de centaines d’agents, multi-actions, pondérations dynamiques.
  - Simplicité des “tricks” Utility pour rendre l’IA souple et non-binaire (cf. “The Simplest AI Trick in the Book”).
  - Architecture hybride Utility/Behavior Tree/GOAP pour combiner flexibilité et contrôle.

- **GDC Vault & GameAIPro Talks**
  - [Improving AI Decision Modeling Through Utility Theory](https://www.gdcvault.com/play/1012410/Improving-AI-Decision-Modeling-Through)
  - [Building a Better Centaur: AI at Massive Scale](https://gdcvault.com/play/1021848/Building-a-Better-Centaur-AI)
  - [Architecture Tricks: Managing Behaviors in Time, Space, and Depth](https://www.gdcvault.com/play/1018040/Architecture-Tricks-Managing-Behaviors-in)
  - [The Simplest AI Trick in the Book](https://gdcvault.com/play/1025281/The-Simplest-AI-Trick-in)

- **Réutilisation de projets antérieurs**
  - Base du GOAP, Influence Map, et clustering directement inspirés de nos prototypes et outils précédents, adaptés pour ce projet RTS.
  - Améliorations sur la propagation mémoire, fog of war, et architecture des clusters.

- **En pratique**
  - Approche hybride, adaptée aux besoins RTS : scoring Utility pour la macro, GOAP/FSM pour l’exécution micro/méso.
  - Intégration des meilleures pratiques issues des conférences et des retours terrain de projets IA modernes.
